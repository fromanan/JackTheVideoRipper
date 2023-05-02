namespace JackTheVideoRipper;

public static class Formats
{
    public static class Video
    {
        public const string AVI = "avi";
        
        public const string MP4 = "mp4";
        
        public const string MOV = "mov";
        
        public const string M4V = "m4v";

        public const string MPEG = "mpeg";

        public const string MKV = "mkv";

        public const string OGG = "ogg";

        public const string FLV = "flv";

        public const string WMV = "wmv";
    }
    
    public static readonly string[] AllVideo =
    {
        Video.AVI,
        Video.MP4,
        Video.MOV,
        Video.M4V,
        Video.MPEG,
        Video.MKV,
        Video.OGG,
        Video.FLV,
        Video.WMV
    };
    
    public static bool IsVideoFormat(string filepath)
    {
        return FileSystem.GetExtension(filepath).ToLower()
            is Video.AVI
            or Video.MP4
            or Video.MOV
            or Video.M4V
            or Video.MPEG
            or Video.MKV
            or Video.OGG
            or Video.FLV
            or Video.WMV;
    }
    
    public static class Audio
    {
        public const string MP3 = "mp3";

        public const string WAV = "wav";

        public const string OGG = "ogg";
        
        public const string M4A = "m4a";

        public const string AAC = "aac";
    }
    
    public static readonly string[] AllAudio =
    {
        Audio.MP3,
        Audio.WAV,
        Audio.OGG,
        Audio.M4A,
        Audio.AAC
    };

    
    public static bool IsAudioFormat(string filepath)
    {
        return FileSystem.GetExtension(filepath).ToLower()
            is Audio.MP3
            or Audio.WAV
            or Audio.OGG
            or Audio.M4A
            or Audio.AAC;
    }

    public static class Image
    {
        public const string WEBP = "webp";

        public const string JPG = "jpg";

        public const string JPEG = "jpeg";

        public const string PNG = "png";

        public const string BMP = "bmp";

        public const string TIFF = "tiff";
        
        public const string TIF = "tif";

        public const string GIF = "gif";

        public const string RAW = "raw";
    }
    
    public static readonly string[] AllImage =
    {
        Image.WEBP,
        Image.JPG,
        Image.JPEG,
        Image.PNG,
        Image.BMP,
        Image.TIFF,
        Image.TIF,
        Image.GIF,
        Image.RAW
    };
    
    public static bool IsImageFormat(string filepath)
    {
        return FileSystem.GetExtension(filepath).ToLower()
            is Image.WEBP
            or Image.JPG
            or Image.JPEG
            or Image.PNG
            or Image.BMP
            or Image.TIFF
            or Image.TIF
            or Image.GIF
            or Image.RAW;
    }

    public static class Pixel
    {
        public const string YUV_420P = "yuv420p";
    }
}