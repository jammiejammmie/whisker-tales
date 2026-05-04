// GameController.cs에 추가할 코드
// 기존 GameController.cs에 다음 메서드들을 추가하거나 수정하세요.

using UnityEngine;

public partial class GameController : MonoBehaviour
{
    /// <summary>
    /// 퍼즐 클리어 시 호출되는 메서드
    /// 카페 복원 시스템 및 오디오 매니저와 연동
    /// </summary>
    public void OnPuzzleClear(int starsEarned, int score)
    {
        Debug.Log($"퍼즐 클리어! 별: +{starsEarned}, 점수: {score}");

        // ========== 기존 코드 ==========
        // 점수 업데이트
        currentScore += score;
        UpdateScoreUI();

        // 이동 횟수 감소
        movesRemaining--;
        UpdateMovesUI();

        // 레벨 목표 확인
        CheckLevelCompletion();

        // ========== 신규 코드: 카페 복원 시스템 연동 ==========
        
        // 카페 복원 시스템에 별 전달
        if (CafeRestorationManager.instance != null)
        {
            CafeRestorationManager.instance.OnPuzzleClear(starsEarned);
            Debug.Log($"카페 복원 시스템에 별 전달: {starsEarned}");
        }
        else
        {
            Debug.LogWarning("CafeRestorationManager를 찾을 수 없습니다!");
        }

        // ========== 신규 코드: 오디오 매니저 연동 ==========
        
        // 매치 성공음 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayMatchSuccess();
            
            // 약간의 딜레이 후 보상 획득음 재생
            Invoke("PlayRewardSound", 0.3f);
        }
        else
        {
            Debug.LogWarning("AudioManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 보상 획득음 재생 (딜레이 후)
    /// </summary>
    private void PlayRewardSound()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayRewardGet();
        }
    }

    /// <summary>
    /// 모든 버튼 클릭 이벤트에 추가할 메서드
    /// 버튼 클릭음을 재생합니다.
    /// </summary>
    public void OnButtonClick()
    {
        // 오디오 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayButtonClick();
        }
        else
        {
            Debug.LogWarning("AudioManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 게임 시작 시 호출
    /// 배경음악 재생
    /// </summary>
    public void StartGame()
    {
        Debug.Log("게임 시작");

        // 배경음악 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM();
        }

        // 기존 게임 시작 코드...
    }

    /// <summary>
    /// 게임 일시정지 시 호출
    /// 배경음악 일시정지
    /// </summary>
    public void PauseGame()
    {
        Debug.Log("게임 일시정지");

        // 배경음악 일시정지
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PauseBGM();
        }

        // 기존 일시정지 코드...
    }

    /// <summary>
    /// 게임 재개 시 호출
    /// 배경음악 재생
    /// </summary>
    public void ResumeGame()
    {
        Debug.Log("게임 재개");

        // 배경음악 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM();
        }

        // 기존 재개 코드...
    }

    /// <summary>
    /// 게임 종료 시 호출
    /// 배경음악 중지
    /// </summary>
    public void EndGame()
    {
        Debug.Log("게임 종료");

        // 배경음악 중지
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
        }

        // 기존 종료 코드...
    }
}

// ========== UI 버튼에 추가할 코드 ==========

/// <summary>
/// 모든 버튼의 OnClick 이벤트에 다음을 추가하세요:
/// 
/// 1. Inspector에서 버튼 선택
/// 2. Button 컴포넌트의 "On Click ()" 섹션 확장
/// 3. "+" 버튼 클릭
/// 4. GameController 오브젝트를 드래그 & 드롭
/// 5. 드롭다운에서 "GameController" → "OnButtonClick()" 선택
/// 
/// 또는 코드에서 다음과 같이 추가:
/// </summary>

public class ButtonClickExample : MonoBehaviour
{
    private Button myButton;

    private void Start()
    {
        myButton = GetComponent<Button>();
        
        // 버튼 클릭 이벤트에 OnButtonClick 메서드 추가
        myButton.onClick.AddListener(() => {
            if (GameController.instance != null)
            {
                GameController.instance.OnButtonClick();
            }
        });
    }
}

// ========== 고양이 상호작용에 추가할 코드 ==========

/// <summary>
/// CatManager.cs의 고양이 상호작용 메서드에 추가하세요
/// </summary>

public partial class CatManager : MonoBehaviour
{
    /// <summary>
    /// 고양이 쓰다듬기
    /// </summary>
    public void PetCat(int catId)
    {
        Debug.Log($"고양이 {catId}를 쓰다듬음");

        // 오디오 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayCatMeow();
        }

        // 고양이 애니메이션 재생
        PlayCatAnimation(catId, "pet");

        // 호감도 증가
        IncreaseCatAffection(catId, 5);

        // 기존 코드...
    }

    /// <summary>
    /// 고양이에게 간식 주기
    /// </summary>
    public void FeedCat(int catId)
    {
        Debug.Log($"고양이 {catId}에게 간식을 줌");

        // 오디오 재생
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayCatMeow();
        }

        // 고양이 애니메이션 재생
        PlayCatAnimation(catId, "eat");

        // 호감도 증가
        IncreaseCatAffection(catId, 10);

        // 기존 코드...
    }

    private void PlayCatAnimation(int catId, string animationType)
    {
        // 고양이 애니메이션 재생 로직
        // TODO: 구현
    }

    private void IncreaseCatAffection(int catId, int amount)
    {
        // 호감도 증가 로직
        // TODO: 구현
    }
}
