namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 매치-3 보드의 타일 타입을 정의하는 열거형
    /// 6가지 종류: 생선, 우유, 털실, 캣닢, 발도장, 생선뼈
    /// </summary>
    public enum TileType
    {
        /// <summary>생선 타일</summary>
        Fish = 0,

        /// <summary>우유 타일</summary>
        Milk = 1,

        /// <summary>털실 타일</summary>
        Yarn = 2,

        /// <summary>캣닢 타일</summary>
        Catnip = 3,

        /// <summary>발도장 타일</summary>
        Pawprint = 4,

        /// <summary>생선뼈 타일</summary>
        Fishbone = 5
    }

    /// <summary>
    /// 특수 아이템 타입을 정의하는 열거형
    /// 매치-3 결과로 생성되는 특수 아이템들
    /// </summary>
    public enum SpecialItemType
    {
        /// <summary>특수 아이템 없음</summary>
        None = 0,

        /// <summary>로켓 (한 줄 제거)</summary>
        Rocket = 1,

        /// <summary>폭탄 (3x3 영역 제거)</summary>
        Bomb = 2,

        /// <summary>무지개 털실 (같은 색 모두 제거)</summary>
        Rainbow = 3,

        /// <summary>망치 (임의의 칸 제거)</summary>
        Hammer = 4
    }

    /// <summary>
    /// 장애물 타입을 정의하는 열거형
    /// 보드에 배치되는 장애물들
    /// </summary>
    public enum ObstacleType
    {
        /// <summary>장애물 없음</summary>
        None = 0,

        /// <summary>상자 (2번 매치로 제거)</summary>
        Box = 1,

        /// <summary>얼음 (1번 매치로 제거)</summary>
        Ice = 2,

        /// <summary>자물쇠 (제거 불가, 매치 불가)</summary>
        Lock = 3,

        /// <summary>체인 (인접 타일과 함께 제거)</summary>
        Chain = 4
    }

    /// <summary>
    /// 매치-3 보드의 개별 타일 데이터
    /// MonoBehaviour를 상속하지 않는 순수 데이터 클래스
    /// </summary>
    public class TileData
    {
        /// <summary>타일의 X 좌표 (0~7)</summary>
        public int x;

        /// <summary>타일의 Y 좌표 (0~7)</summary>
        public int y;

        /// <summary>타일의 타입 (색상)</summary>
        public TileType type;

        /// <summary>타일이 가진 특수 아이템</summary>
        public SpecialItemType specialItem;

        /// <summary>타일이 가진 장애물</summary>
        public ObstacleType obstacle;

        /// <summary>장애물의 내구도 (Box, Ice, Chain 등)</summary>
        public int obstacleHealth;

        /// <summary>이 타일이 매치되었는지 여부</summary>
        public bool isMatched;

        /// <summary>이 타일이 현재 이동 중인지 여부</summary>
        public bool isMoving;

        /// <summary>이 타일이 잠겨있는지 여부 (Lock 장애물)</summary>
        public bool isLocked;

        /// <summary>
        /// TileData 생성자
        /// </summary>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        /// <param name="type">타일 타입</param>
        public TileData(int x, int y, TileType type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
            this.specialItem = SpecialItemType.None;
            this.obstacle = ObstacleType.None;
            this.obstacleHealth = 0;
            this.isMatched = false;
            this.isMoving = false;
            this.isLocked = false;
        }

        /// <summary>
        /// 이 타일이 다른 타일과 인접한지 확인
        /// </summary>
        /// <param name="other">비교할 다른 타일</param>
        /// <returns>인접하면 true, 아니면 false</returns>
        public bool IsAdjacentTo(TileData other)
        {
            if (other == null)
                return false;

            int dx = System.Math.Abs(this.x - other.x);
            int dy = System.Math.Abs(this.y - other.y);

            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}