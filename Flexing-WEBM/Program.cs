using System.Diagnostics;

namespace Flexing_WEBM
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string Source = args[0];
                if (!File.Exists(Source))
                {
                    Console.WriteLine($"Source file in path {Source} doesn't exists!");
                    Process.GetCurrentProcess().Kill();
                }
                WorkContainer.StartUp(Source);
            }
            catch
            {
                Console.WriteLine("Check arguments!");
                Console.ReadLine();
            }

        }
        internal class WorkContainer
        {
            public static void StartUp(string Source)
            {   
                string CurrentDirectory = Environment.CurrentDirectory;
                string RandomTempFolderIdentity = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 5);
                Directory.CreateDirectory($"{CurrentDirectory}/Temp{RandomTempFolderIdentity}");
                Directory.CreateDirectory($"{CurrentDirectory}/Temp{RandomTempFolderIdentity}/img/");
                Directory.CreateDirectory($"{CurrentDirectory}/Temp{RandomTempFolderIdentity}/webmImg");
                Directory.CreateDirectory($"{CurrentDirectory}/Temp{RandomTempFolderIdentity}/imgRes/");
                string[] resolution = ffmpegWork.ffmprobeResolution(Source).Split(" ");
                int maxWidth = Convert.ToUInt16(resolution[0]);
                int maxHeight = Convert.ToUInt16(resolution[1]);
                int framerate = resolution[2].Contains('/') ? (Convert.ToInt16(resolution[2].Split('/')[0]) / Convert.ToInt16(resolution[2].Split('/')[1])) : Convert.ToInt16(resolution[2]);
                
                        ffmpegWork.ffmpegAudio(Source, CurrentDirectory, RandomTempFolderIdentity);
                        ffmpegWork.ffmpegPngFrame(Source, CurrentDirectory, RandomTempFolderIdentity);
                        ffmpegWork.ffmpegPngChangeResolution(CurrentDirectory, RandomTempFolderIdentity, maxWidth, maxHeight, framerate);
                        ffmpegWork.ffmpegPngToWebmFrame(CurrentDirectory, RandomTempFolderIdentity);
                        ffmpegWork.ffmpegWebmFrameToVideo(CurrentDirectory, RandomTempFolderIdentity, framerate);
            }
        }
        internal class ffmpegWork
        { 
            static public void ffmpegWebmFrameToVideo(string CurrentDirectory, string RandomTempFolderIdentity, int framerate)
            {
                Console.WriteLine("begin render video");
                try
                {
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = $"{CurrentDirectory}/ffmpeg/bin/ffmpeg";
                    compiler.StartInfo.Arguments = $" -y -f concat -safe 0 -i {CurrentDirectory}/Temp{RandomTempFolderIdentity}/concatFiles.txt -i {CurrentDirectory}/Temp{RandomTempFolderIdentity}/sound.opus -framerate {framerate} -c copy {CurrentDirectory}/Temp{RandomTempFolderIdentity}/output.webm";
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.UseShellExecute = false;
                    compiler.Start();
                    compiler.WaitForExit();
                    Console.WriteLine($"render video finished");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            static public void ffmpegPngToWebmFrame(string CurrentDirectory, string RandomTempFolderIdentity )
            {
                
                int ImgValues = new DirectoryInfo($"{CurrentDirectory}/Temp{RandomTempFolderIdentity}/imgRes/").GetFiles().Length;
                for(int i = 0; i < ImgValues; i++)
                {
                    try
                    {
                        Process compiler = new Process();
                        compiler.StartInfo.FileName = $"{CurrentDirectory}/ffmpeg/bin/ffmpeg";
                        compiler.StartInfo.Arguments = $"-i {CurrentDirectory}/Temp{RandomTempFolderIdentity}/imgRes/img-{i+1}.png -c:a libvorbis {CurrentDirectory}/Temp{RandomTempFolderIdentity}/webmImg/frame-{i+1}.webm";
                        compiler.StartInfo.RedirectStandardOutput = true;
                        compiler.StartInfo.CreateNoWindow = true;
                        compiler.StartInfo.UseShellExecute = false;
                        compiler.Start();
                        compiler.WaitForExit();
                        Console.WriteLine($"convert frame to webm - {i+1}/{ImgValues}");
                        StreamWriter writer = new StreamWriter($"{CurrentDirectory}/Temp{RandomTempFolderIdentity}/concatFiles.txt", true);
                        writer.WriteLine($"file '{CurrentDirectory}/Temp{RandomTempFolderIdentity}/webmImg/frame-{i+1}.webm'");
                        writer.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            static public void ffmpegAudio(string Source, string CurrentDirectory, string RandomTempFolderIdentity)
            {
                Console.WriteLine("trying get the audio");
                try
                {
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = $"{CurrentDirectory}/ffmpeg/bin/ffmpeg";
                    compiler.StartInfo.Arguments = $"-i {Source} {CurrentDirectory}/Temp{RandomTempFolderIdentity}/sound.opus";
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.UseShellExecute = false;
                    compiler.Start();
                    compiler.WaitForExit();
                    Console.WriteLine("audio received");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            static public void ffmpegPngFrame(string Source, string CurrentDirectory, string RandomTempFolderIdentity)
            {
                Console.WriteLine("parsing of the video into frames begin");
                try
                {
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = $"{Environment.CurrentDirectory}/ffmpeg/bin/ffmpeg";
                    compiler.StartInfo.Arguments = $"-i {Source} {CurrentDirectory}/Temp{RandomTempFolderIdentity}/img/img-%d.png";
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.UseShellExecute = false;
                    compiler.Start();
                    compiler.WaitForExit();
                    Console.WriteLine("parsing of the video into frames is completed");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            static public int index;
            static public void ffmpegPngChangeResolution(string CurrentDirectory, string RandomTempFolderIdentity, int maxWidth, int maxHeight, int framerate)
            {
                int ImgValues = new DirectoryInfo($"{CurrentDirectory}/Temp{RandomTempFolderIdentity}/img/").GetFiles().Length;
                int delta = 5;
                double bouncesPerSecond = 0.7;

                for (int i = 0; i < ImgValues; i++,index++)
                {
                    int height = (int)(index == 0 ? maxHeight : (Math.Floor(Math.Abs(Math.Cos(index / (framerate / bouncesPerSecond) * Math.PI) * (maxHeight - delta))) + delta));

                    try
                    { 
                        Process compiler = new Process();
                        compiler.StartInfo.FileName = $"{CurrentDirectory}/ffmpeg/bin/ffmpeg";
                        compiler.StartInfo.Arguments = $"-y -i {CurrentDirectory}/Temp{RandomTempFolderIdentity}/img/img-{i + 1}.png -vf scale={maxWidth}:{height} -c:a libvorbis {CurrentDirectory}/Temp{RandomTempFolderIdentity}/imgRes/img-{i + 1}.png";
                        compiler.StartInfo.RedirectStandardOutput = true;
                        compiler.StartInfo.CreateNoWindow = true;
                        compiler.StartInfo.UseShellExecute = false;
                        compiler.Start();
                        Console.WriteLine($"png frame resolution changed - {i + 1}/{ImgValues}   height={height}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                System.Threading.Thread.Sleep(1500);
            }
            static public string ffmprobeResolution(string Source)
            {
                try
                {
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = $"{Environment.CurrentDirectory}/ffmpeg/bin/ffprobe";
                    compiler.StartInfo.Arguments = $"-v error -select_streams v:0 -show_entries stream=width,height,r_frame_rate -of default=nw=1 {Source}";
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.UseShellExecute = false;
                    compiler.Start();
                    string output = compiler.StandardOutput.ReadToEnd();
                    compiler.WaitForExit();
                    string[] resData = output.Split("\r\n");
                    string cmdData = $"{resData[0].Split("=")[1]} {resData[1].Split("=")[1]} {resData[2].Split("=")[1]}";
                    return cmdData;                   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return e.Message;
                }
            }
        }
    }
}


