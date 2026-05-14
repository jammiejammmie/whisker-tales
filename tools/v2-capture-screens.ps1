# Automated V2 screen capture sequence.
# Boots the V2 APK on the connected device, walks through each screen via adb taps,
# saves a PNG per screen to ./screenshots/v2/<timestamp>/.
#
# Requires: device with USB debugging, V2 APK already installed.
# Run from project root or anywhere — paths are absolute.

$ErrorActionPreference = "Stop"

# --- Config ---
$adb       = "C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
$pkg       = "com.nyangstudio.whiskertales"
$projectRoot = "C:\whisker-tales-master\whisker-tales"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outDir    = Join-Path $projectRoot "screenshots\v2\$timestamp"

# Tap coordinates (1080x2340 device, V2 SafeArea-shifted +80 top margin).
# V2-16 layout: empty Home center (Play removed), scene-level tab bar at bottom.
# LevelSelect / Gameplay no longer have UI entry — they exist but require code-side nav.
$Buttons = @{
    HomeTab       = @(185, 2240)   # bottom bar tab x=-360
    CatRoom       = @(422, 2240)   # bottom bar tab x=-120
    Cafe          = @(658, 2240)   # bottom bar tab x=+120
    Meditation    = @(894, 2240)   # bottom bar tab x=+360
    Settings      = @(945,  315)   # top-right gear, +80 SafeArea shift
    Back          = @(130,  330)   # top-left arrow, +80 SafeArea shift
}

# --- Helpers ---
function Invoke-Adb {
    param([string[]]$Args)
    & $adb @Args | Out-Null
}

function Assert-Foreground {
    param([string]$Stage)
    $focus = & $adb shell "dumpsys window | grep mCurrentFocus"
    if ($focus -notmatch [regex]::Escape($pkg))
    {
        throw "Foreground check failed at '$Stage'. Current focus: $focus"
    }
}

function Capture-Screen {
    param([string]$Name)
    # Hard guard: never capture if our app isn't focused. Prevents leaking unrelated
    # apps' contents into the screenshot archive if the user happens to be elsewhere.
    Assert-Foreground -Stage $Name

    $remote = "/sdcard/v2_cap.png"
    $local  = Join-Path $outDir "$Name.png"
    & $adb shell screencap -p $remote | Out-Null
    & $adb pull $remote $local | Out-Null
    & $adb shell rm $remote | Out-Null
    Write-Output ("  [{0}] {1}" -f $Name, $local)
}

function Tap {
    param([string]$Name, [int]$WaitMs = 1500)
    $coords = $Buttons[$Name]
    if (-not $coords)
    {
        throw "Unknown button: $Name"
    }
    & $adb shell input tap $coords[0] $coords[1] | Out-Null
    Start-Sleep -Milliseconds $WaitMs
}

# --- Preflight ---
Write-Output "[v2-capture] Device check"
$devicesRaw = & $adb devices
if (-not ($devicesRaw -match "\sdevice$"))
{
    throw "No device authorized. Connect phone with USB debugging."
}

New-Item -ItemType Directory -Path $outDir -Force | Out-Null
Write-Output ("[v2-capture] Output: {0}" -f $outDir)

# --- Boot fresh ---
Write-Output "[v2-capture] Boot $pkg (force-stop + launch + wait for focus)"
Invoke-Adb @("shell", "am", "force-stop", $pkg)
# Without this small gap, am start can attach to a not-yet-fully-killed instance and resume
# at whatever screen was last open (e.g. LevelSelect from the previous run).
Start-Sleep -Milliseconds 1500
# am start -W blocks until the activity is up. More reliable than fire-and-forget +
# polling — especially on cold-start after a fresh install when IL2CPP first-frame is slow.
$activity = "$pkg/com.unity3d.player.UnityPlayerActivity"
& $adb shell am start -W -n $activity | Out-Null

# Even with -W, give Boot_Persistent → Main_App additive load a beat to land focus.
$focusOk = $false
for ($i = 0; $i -lt 40; $i++)
{
    Start-Sleep -Milliseconds 500
    $focus = & $adb shell "dumpsys window | grep mCurrentFocus"
    if ($focus -match [regex]::Escape($pkg))
    {
        $focusOk = $true
        break
    }
}
if (-not $focusOk)
{
    throw "App never reached foreground after launch. Check device state."
}
# Generous post-focus wait: Boot_Persistent IL2CPP init + Main_App additive load + ScreenNavigator
# initial fade-in. Earlier 2s sometimes captured a transition frame.
Start-Sleep -Seconds 4

# --- Walk screens ---
Write-Output "[v2-capture] Capturing..."

Capture-Screen "01_home"

Tap "CatRoom"; Capture-Screen "02_catroom"
Tap "Back"; Start-Sleep -Milliseconds 700

Tap "Cafe"; Capture-Screen "03_cafe"
Tap "Back"; Start-Sleep -Milliseconds 700

Tap "Meditation"; Capture-Screen "04_sleepmode"
Tap "Back"; Start-Sleep -Milliseconds 700

Tap "Settings"; Capture-Screen "05_settings"
Tap "Back"; Start-Sleep -Milliseconds 700

# Final home snapshot (validates clean nav back to home).
Capture-Screen "06_home_return"

Write-Output ""
$fileCount = (Get-ChildItem $outDir).Count
Write-Output ("[v2-capture] Done. {0} files in {1}" -f $fileCount, $outDir)
Write-Output ""
Write-Output "Skipped (no UI entry in V2-16):"
Write-Output "  - LevelSelect / Gameplay — Play CTA removed during home cleanup."
Write-Output "  - LevelClear / GameFail — require real match-3 result."
