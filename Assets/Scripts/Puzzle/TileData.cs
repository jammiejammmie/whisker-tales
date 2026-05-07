namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// 留ㅼ튂-3 蹂대뱶???????낆쓣 ?뺤쓽?섎뒗 ?닿굅??
    /// 6媛吏 ?됱긽: ?앹꽑, ?곗쑀, ?몄떎, 罹ｋ떌, 諛쒕룄?? ?앹꽑堉?
    /// </summary>
    public enum TileType
    {
        /// <summary>?앹꽑 ???/summary>
        Fish = 0,

        /// <summary>?곗쑀 ???/summary>
        Milk = 1,

        /// <summary>?몄떎 ???/summary>
        Yarn = 2,

        /// <summary>罹ｋ떌 ???/summary>
        Catnip = 3,

        /// <summary>諛쒕룄?????/summary>
        Pawprint = 4,

        /// <summary>?앹꽑堉????/summary>
        Fishbone = 5
    }

    /// <summary>
    /// ?뱀닔 ?꾩씠????낆쓣 ?뺤쓽?섎뒗 ?닿굅??
    /// 留ㅼ튂-3 寃곌낵濡??앹꽦?섎뒗 ?뱀닔 ?꾩씠?쒕뱾
    /// </summary>
    public enum SpecialItemType
    {
        /// <summary>?뱀닔 ?꾩씠???놁쓬</summary>
        None = 0,

        /// <summary>濡쒖폆 (??以??쒓굅)</summary>
        Rocket = 1,

        /// <summary>??깂 (3횞3 ?곸뿭 ?쒓굅)</summary>
        Bomb = 2,

        /// <summary>臾댁?媛??몄떎 (媛숈? ??紐⑤몢 ?쒓굅)</summary>
        Rainbow = 3,

        /// <summary>?λ깷 留앹튂 (?꾩쓽 移??쒓굅)</summary>
        Hammer = 4
    }

    /// <summary>
    /// ?μ븷臾???낆쓣 ?뺤쓽?섎뒗 ?닿굅??
    /// ?덈꺼??諛곗튂?섎뒗 ?μ븷臾쇰뱾
    /// </summary>
    public enum ObstacleType
    {
        /// <summary>?μ븷臾??놁쓬</summary>
        None = 0,

        /// <summary>?곸옄 (2???곗튂濡??쒓굅)</summary>
        Box = 1,

        /// <summary>?쇱쓬 (1???곗튂濡??쒓굅)</summary>
        Ice = 2,

        /// <summary>?먮Ъ??(?쒓굅 遺덇?, 留ㅼ튂 遺덇?)</summary>
        Lock = 3,

        /// <summary>泥댁씤 (?몄젒 ??쇨낵 ?④퍡 ?쒓굅)</summary>
        Chain = 4
    }

    /// <summary>
    /// 留ㅼ튂-3 蹂대뱶??媛쒕퀎 ????곗씠??
    /// MonoBehaviour瑜??곸냽?섏? ?딅뒗 ?쒖닔 ?곗씠???대옒??
    /// </summary>
    public class TileData
    {
        /// <summary>??쇱쓽 X 醫뚰몴 (0~7)</summary>
        public int x;

        /// <summary>??쇱쓽 Y 醫뚰몴 (0~7)</summary>
        public int y;

        /// <summary>??쇱쓽 ???(?됱긽)</summary>
        public TileType type;

        /// <summary>??쇱씠 媛吏??뱀닔 ?꾩씠??/summary>
        public SpecialItemType specialItem;

        /// <summary>??쇱씠 媛吏??μ븷臾?/summary>
        public ObstacleType obstacle;

        /// <summary>?μ븷臾쇱쓽 ?닿뎄??(Box, Ice, Chain ??</summary>
        public int obstacleHealth;

        /// <summary>????쇱씠 留ㅼ튂?섏뿀?붿? ?щ?</summary>
        public bool isMatched;

        /// <summary>????쇱씠 ?꾩옱 ?대룞 以묒씤吏 ?щ?</summary>
        public bool isMoving;

        /// <summary>????쇱씠 ?좉꺼?덈뒗吏 ?щ? (Lock ?μ븷臾?</summary>
        public bool isLocked;

        /// <summary>
        /// TileData ?앹꽦??
        /// </summary>
        /// <param name="x">X 醫뚰몴</param>
        /// <param name="y">Y 醫뚰몴</param>
        /// <param name="type">??????/param>
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
        /// ????쇱씠 ?ㅻⅨ ??쇨낵 ?몄젒?쒖? ?뺤씤
        /// </summary>
        /// <param name="other">鍮꾧탳???ㅻⅨ ???/param>
        /// <returns>?몄젒?섎㈃ true, ?꾨땲硫?false</returns>
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
