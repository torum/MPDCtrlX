using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MPDCtrlX.Models;
using MPDCtrlX.Services.Contracts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static MPDCtrlX.Services.MpcService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MPDCtrlX.Services;

public class BinaryDownloader : IBinaryDownloader
{
    private static TcpClient _binaryConnection = new();
    private StreamReader? _binaryReader;
    private StreamWriter? _binaryWriter;

    private AlbumImage _albumCover = new();
    public AlbumImage AlbumCover { get => _albumCover; }

    private string _host = "";
    private int _port = 6600;

    private string? MpdVersion { get; set; }

    public BinaryDownloader()
    {

    }

    public async Task<bool> MpdBinaryConnectionStart(string host, int port, string password)
    {
        ConnectionResult r = await MpdBinaryConnect(host, port);

        if (r.IsSuccess)
        {
            CommandResult d = await MpdBinarySendPassword(password);

            if (d.IsSuccess)
            {
                // ここでIdleにして、以降はnoidle + cmd + idleの組み合わせでやる。
                // ただし、実際にはidleのあとReadしていないからタイムアウトで切断されてしまう模様。

                // awaitが必要だった。
                //d = await MpdSendIdle();

                return d.IsSuccess;
            }
        }

        return false;
    }

    private async Task<ConnectionResult> MpdBinaryConnect(string host, int port)
    {
        ConnectionResult result = new();

        _binaryConnection = new TcpClient();

        _host = host;
        _port = port;

        try
        {
            await _binaryConnection.ConnectAsync(_host, _port);

            if (_binaryConnection.Client is null)
            {
                Debug.WriteLine("_binaryConnection.Client is null. " + host + " " + port.ToString());

                result.ErrorMessage = "_binaryConnection.Client is null";

                return result;
            }

            if (_binaryConnection.Client.Connected)
            {
                var tcpStream = _binaryConnection.GetStream();

                tcpStream.ReadTimeout = 3000;

                _binaryReader = new(tcpStream);
                _binaryWriter = new(tcpStream)
                {
                    AutoFlush = true
                };

                string? response = await _binaryReader.ReadLineAsync();
                if (response is not null)
                {
                    if (response.StartsWith("OK MPD "))
                    {
                        MpdVersion = response.Replace("OK MPD ", string.Empty).Trim();

                        result.IsSuccess = true;

                        // Done for now.
                    }
                    else
                    {
                        Debug.WriteLine("**** TCP Binary Connection: MPD did not respond with proper respose.@MpdBinaryConnect");
                    }
                }
                else
                {
                    Debug.WriteLine("**** TCP Binary Connection: MPD did not respond with proper respose. @MpdBinaryConnect");
                }
            }
            else
            {
                Debug.WriteLine("**** !client.Client.Connected@MpdBinaryConnect");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("**** Exception@MpdBinaryConnect: " + e.Message);
        }

        return result;
    }

    private async Task<CommandResult> MpdBinarySendPassword(string password = "")
    {
        CommandResult ret = new();

        if (string.IsNullOrEmpty(password))
        {
            ret.IsSuccess = true;
            ret.ResultText = "OK";
            ret.ErrorMessage = "";

            return ret;
        }

        string cmd = "password " + password + "\n";

        return await MpdBinarySendCommand(cmd);
    }

    private async Task<CommandResult> MpdBinarySendCommand(string cmd)
    {
        CommandResult ret = new();

        if (_binaryConnection.Client is null)
        {
            return ret;
        }

        if ((_binaryWriter is null) || (_binaryReader is null))
        {
            Debug.WriteLine("@MpdBinarySendCommand: " + "_commandWriter or _commandReader is null");

            return ret;
        }

        if (!_binaryConnection.Client.Connected)
        {
            Debug.WriteLine("@MpdBinarySendCommand: " + "NOT IsMpdCommandConnected");

            return ret;
        }

        // WriteAsync
        try
        {
            //IsBusy?.Invoke(this, true);

            string cmdDummy = cmd;
            if (cmd.StartsWith("password "))
                cmdDummy = "password ****";

            cmdDummy = cmdDummy.Trim().Replace("\n", "\n" + ">>>>");

            await _binaryWriter.WriteAsync(cmd.Trim() + "\n");
        }
        catch (System.IO.IOException e)
        {
            // IOException : Unable to write data to the transport connection
            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            //IsBusy?.Invoke(this, false);
            return ret;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception@MpdBinarySendCommand: " + cmd.Trim() + " WriteAsync " + e.Message);

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            //IsBusy?.Invoke(this, false);
            return ret;
        }

        // ReadLineAsync
        try
        {
            //IsBusy?.Invoke(this, true);

            StringBuilder stringBuilder = new();

            string ackText = "";
            string errText = "";
            bool isNullReturn = false;

            while (true)
            {
                string? line = await _binaryReader.ReadLineAsync();

                if (line is not null)
                {
                    if (line.StartsWith("ACK"))
                    {
                        Debug.WriteLine("ACK line @MpdBinarySendCommand: " + cmd.Trim() + " and " + line);

                        if (!string.IsNullOrEmpty(line))
                            stringBuilder.Append(line + "\n");

                        ret.ErrorMessage = line;
                        ackText = line;
                        //isAck = true;

                        break;
                    }
                    else if (line.StartsWith("OK"))
                    {
                        ret.IsSuccess = true;

                        if (!string.IsNullOrEmpty(line))
                            stringBuilder.Append(line + "\n");

                        break;
                    }
                    else if (line.StartsWith("error"))
                    {
                        Debug.WriteLine("error line @MpdBinarySendCommand: " + cmd.Trim() + " and " + line);

                        //isErr = true;
                        errText = line;
                        ret.ErrorMessage = line;

                        if (!string.IsNullOrEmpty(line))
                            stringBuilder.Append(line + "\n");
                    }
                    else if (line.StartsWith("changed: "))
                    {
                        if (!string.IsNullOrEmpty(line))
                            stringBuilder.Append(line + "\n");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            stringBuilder.Append(line + "\n"); // << has to be \n
                        }
                    }
                }
                else
                {
                    isNullReturn = true;

                    break;
                }
            }

            if (isNullReturn)
            {
                Debug.WriteLine("@MpdBinarySendCommand ReadLineAsync isNullReturn");

                //IsBusy?.Invoke(this, false);
                return ret;
            }
            else
            {
                ret.ResultText = stringBuilder.ToString();

                //IsBusy?.Invoke(this, false);
                return ret;
            }

        }
        catch (System.InvalidOperationException e)
        {
            // The stream is currently in use by a previous operation on the stream.

            Debug.WriteLine("InvalidOperationException@MpdBinarySendCommand: " + cmd.Trim() + " ReadLineAsync ---- " + e.Message);

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            //IsBusy?.Invoke(this, false);
            return ret;
        }
        catch (System.IO.IOException e)
        {
            // IOException : Unable to write data to the transport connection

            Debug.WriteLine("IOException@MpdBinarySendCommand: " + cmd.Trim() + " ReadLineAsync ---- " + e.Message);

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            //IsBusy?.Invoke(this, false);
            return ret;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception@MpdBinarySendCommand: " + cmd.Trim() + " ReadLineAsync ---- " + e.Message);

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            //IsBusy?.Invoke(this, false);
            return ret;
        }

    }

    private async Task<CommandBinaryResult> MpdBinarySendBinaryCommand(string cmd)
    {
        CommandBinaryResult ret = new();

        if (_binaryConnection.Client is null)
        {
            Debug.WriteLine("@MpdBinarySendBinaryCommand: " + "TcpClient.Client is null");

            ret.IsSuccess = false;
            ret.ErrorMessage = "TcpClient.Client is null";

            return ret;
        }

        if ((_binaryWriter is null) || (_binaryReader is null))
        {
            Debug.WriteLine("@MpdBinarySendBinaryCommand: " + "_binaryWriter or _binaryReader is null");

            ret.IsSuccess = false;
            ret.ErrorMessage = "_binaryWriter or _binaryReader is null";

            return ret;
        }

        if (!_binaryConnection.Client.Connected)
        {
            Debug.WriteLine("@MpdBinarySendBinaryCommand: " + "NOT IsMpdBinaryCommandConnected");

            ret.IsSuccess = false;
            ret.ErrorMessage = "NOT IsMpdBinaryCommandConnected";

            return ret;
        }

        // WriteAsync
        try
        {
            string cmdDummy = cmd;
            if (cmd.StartsWith("password "))
                cmdDummy = "password ****";

            cmdDummy = cmdDummy.Trim().Replace("\n", "\n" + ">>>>");

            await _binaryWriter.WriteAsync(cmd.Trim() + "\n");
        }
        catch (System.IO.IOException e)
        {
            // IOException : Unable to write data to the transport connection
            
            Debug.WriteLine("Exception@MpdBinarySendBinaryCommand: " + cmd.Trim() + " WriteAsync " + e.Message);

            ret.IsTimeOut = true; // re-try.

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            return ret;
            
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception@MpdBinarySendBinaryCommand: " + cmd.Trim() + " WriteAsync " + e.Message);

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            return ret;
        }

        // ReadAsync
        try
        {
            StringBuilder stringBuilder = new();

            byte[] bin = [];

            bool isAck = false;
            string ackText = "";
            bool isErr = false;
            string errText = "";
            bool isNullReturn = false;
            bool isBinaryFound = false;

            bool isWaitForOK = true;

            while (isWaitForOK)
            {
                int readSize = 0;
                int bufferSize = 5000;
                byte[] buffer = new byte[bufferSize];
                byte[] bindata = [];

                using (MemoryStream ms = new())
                {
                    while ((readSize = await _binaryReader.BaseStream.ReadAsync(buffer)) > 0)
                    {
                        ms.Write(buffer, 0, readSize);

                        if (readSize < bufferSize)
                        {
                            break;
                        }
                    }

                    bindata = ms.ToArray();
                }

                if (bindata.Length == 0)
                {
                    isNullReturn = true;

                    isWaitForOK = false;
                }
                else
                {
                    bin = CombineByteArray(bin, bindata);

                    string res = Encoding.Default.GetString(bindata, 0, bindata.Length);

#pragma warning disable IDE0305 
                    List<string> values = res.Split("\n").ToList();
#pragma warning restore IDE0305 

                    foreach (var line in values)
                    {
                        if (line is not null)
                        {
                            if (line.StartsWith("ACK"))
                            {
                                Debug.WriteLine("ack line @MpdBinarySendCommand: " + cmd.Trim() + " and " + line);

                                if (!string.IsNullOrEmpty(line))
                                    stringBuilder.Append(line + "\n");

                                ret.ErrorMessage = line;
                                ackText = line;
                                isAck = true;

                                isWaitForOK = false;

                                break;
                            }
                            else if (line.StartsWith("error"))
                            {
                                Debug.WriteLine("error line @MpdBinarySendCommand: " + cmd.Trim() + " and " + line);

                                isErr = true;
                                errText = line;
                                ret.ErrorMessage = line;

                                if (!string.IsNullOrEmpty(line))
                                    stringBuilder.Append(line + "\n");
                            }
                            else if (line.StartsWith("changed: "))
                            {
                                if (!string.IsNullOrEmpty(line))
                                    stringBuilder.Append(line + "\n");
                            }
                            else if (line.StartsWith("size: "))
                            {
                                if (!string.IsNullOrEmpty(line))
                                    stringBuilder.Append(line + "\n");
#pragma warning disable IDE0305
                                List<string> s = line.Split(':').ToList();
#pragma warning restore IDE0305
                                if (s.Count > 1)
                                {
                                    if (Int32.TryParse(s[1], out int i))
                                    {
                                        ret.WholeSize = i;
                                    }
                                }
                            }
                            else if (line.StartsWith("type: "))
                            {
                                if (!string.IsNullOrEmpty(line))
                                    stringBuilder.Append(line + "\n");
#pragma warning disable IDE0305
                                List<string> s = line.Split(':').ToList();
#pragma warning restore IDE0305
                                if (s.Count > 1)
                                {
                                    ret.Type = s[1].Trim();
                                }
                            }
                            else if (line.StartsWith("binary: "))
                            {
                                isBinaryFound = true;

                                if (!string.IsNullOrEmpty(line))
                                    stringBuilder.Append(line + "\n");

                                stringBuilder.Append("{binary data}" + "\n");
#pragma warning disable IDE0305
                                List<string> s = line.Split(':').ToList();
#pragma warning restore IDE0305
                                if (s.Count > 1)
                                {
                                    if (Int32.TryParse(s[1], out int i))
                                    {
                                        ret.ChunkSize = i;
                                    }
                                }
                            }
                            else if (line == "OK")
                            {
                                ret.IsSuccess = true;

                                if (!string.IsNullOrEmpty(line))
                                    stringBuilder.Append(line + "\n");

                                isWaitForOK = false;
                                break;
                            }
                            else
                            {
                                // This should be binary data if not reading correctly above.
                            }
                        }
                        else
                        {
                            isNullReturn = true;

                            isWaitForOK = false;
                            break;
                        }
                    }
                }
            }

            if (isNullReturn)
            {
                Debug.WriteLine("@MpdBinarySendBinaryCommand ReadAsync isNullReturn");

                ret.ErrorMessage = "ReadAsync@MpdBinarySendBinaryCommand received null data";
                ret.IsSuccess = false;
                return ret;
            }
            else
            {
                if (isAck)
                {
                    // Ignore "file not exists" for now.
                    //MpdAckError?.Invoke(this, ackText + " (@MCGB)");

                    Debug.WriteLine("@MpdBinarySendBinaryCommand ReadAsync isAck");

                    ret.ErrorMessage = ackText + " (@MpdBinarySendBinaryCommand)";
                    //ret.IsSuccess = false;
                    //return ret;
                }

                if (isErr)
                {
                    //MpdFatalError?.Invoke(this, ackText + " (@MCGB)");

                    Debug.WriteLine("@MpdBinarySendBinaryCommand ReadAsync isErr");

                    ret.ErrorMessage = errText + " (@MpdBinarySendBinaryCommand)";
                    //ret.IsSuccess = false;
                    //return ret;
                }

                if (isBinaryFound)
                {
                    return ParseAlbumImageData(bin);
                }
                else
                {
                    ret.ErrorMessage = "No binary data found. AlbumArt doesn't exists or ... needs to be a albumart command instead of readpicture? (@MpdBinarySendBinaryCommand)";
                    ret.IsSuccess = false;
                    return ret;
                }
            }
        }
        catch (System.InvalidOperationException e)
        {
            // The stream is currently in use by a previous operation on the stream.

            Debug.WriteLine("InvalidOperationException@MpdBinarySendBinaryCommand: " + cmd.Trim() + " ReadAsync ---- " + e.Message);

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            return ret;
        }
        catch (System.IO.IOException e)
        {
            // IOException : Unable to write data to the transport connection

            Debug.WriteLine("IOException@MpdBinarySendBinaryCommand: " + cmd.Trim() + " ReadAsync ---- " + e.Message);

            ret.IsTimeOut = true; // re-try.

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            return ret;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception@MpdBinarySendBinaryCommand: " + cmd.Trim() + " ReadAsync ---- " + e.Message);

            ret.IsSuccess = false;
            ret.ErrorMessage = e.Message;

            return ret;
        }
    }

    private CommandBinaryResult ParseAlbumImageData(byte[] data)
    {
        CommandBinaryResult r = new();

        //if (MpdStop) return r;

        if (data.Length > 20000000) //2000000000
        {
            Debug.WriteLine("**ParseAlbumImageData: binary file size too big: " + data.Length.ToString());
            r.IsSuccess = false;
            r.ErrorMessage = "ParseAlbumImageData: binary file size too big: \" + data.Length.ToString()";
            _albumCover.IsDownloading = false;
            return r;
        }

        if (string.IsNullOrEmpty(_albumCover.SongFilePath))
        {
            Debug.WriteLine("**ParseAlbumImageData: File path is not set.");

            _albumCover.IsDownloading = false;
            return r;
        }

        if (!_albumCover.IsDownloading)
        {
            Debug.WriteLine("**ParseAlbumImageData: IsDownloading = false. Downloading canceld? .");

            _albumCover.IsDownloading = false;
            return r;
        }

        try
        {
            int gabStart = 0;
            int gabEnd = 0;

            string res = Encoding.Default.GetString(data, 0, data.Length);

            int binSize = 0;
            int binResSize = 0;

#pragma warning disable IDE0305
            List<string> values = res.Split('\n').ToList();
#pragma warning restore IDE0305

            bool found = false;
            foreach (var val in values)
            {
                if (val.StartsWith("size: "))
                {
                    found = true;

                    gabStart = gabStart + val.Length + 1;
#pragma warning disable IDE0305
                    List<string> s = val.Split(':').ToList();
#pragma warning restore IDE0305
                    if (s.Count > 1)
                    {
                        if (Int32.TryParse(s[1], out int i))
                        {
                            binSize = i;

                            r.WholeSize = i;
                        }
                    }
                }
                else if (val.StartsWith("type: "))
                {
                    gabStart = gabStart + val.Length + 1;
#pragma warning disable IDE0305
                    List<string> s = val.Split(':').ToList();
#pragma warning restore IDE0305
                    if (s.Count > 1)
                    {
                        r.Type = s[1];
                    }

                }
                else if (val.StartsWith("binary: "))
                {
                    gabStart = gabStart + val.Length + 1;
#pragma warning disable IDE0305
                    List<string> s = val.Split(':').ToList();
#pragma warning restore IDE0305
                    if (s.Count > 1)
                    {
                        if (Int32.TryParse(s[1], out int i))
                        {
                            binResSize = i;

                            r.ChunkSize = binResSize;
                        }
                    }
                }
                else if (val == "OK")
                {
                    if (found)
                    {
                        gabEnd = gabEnd + val.Length + 1;
                    }
                    else
                    {
                        gabStart = gabStart + val.Length + 1;
                    }
                }
                else if (val.StartsWith("ACK"))
                {
                    // ACK shouldn't be here.

                    if (found)
                    {
                        gabEnd = gabEnd + val.Length + 1;
                    }
                    else
                    {
                        gabStart = gabStart + val.Length + 1;
                    }
                }
                else if (val.StartsWith("changed:"))
                {
                    if (found)
                    {
                        gabEnd = gabEnd + val.Length + 1;
                    }
                    else
                    {
                        gabStart = gabStart + val.Length + 1;
                    }
                }
                else
                {
                    // should be binary...
                }
            }

            gabEnd++;

            if (binSize > 20000000)
            {
                Debug.WriteLine("binary file too big: " + binSize.ToString() + " " + _albumCover.SongFilePath);

                _albumCover.IsDownloading = false;

                return r;
            }

            if ((binSize == 0))
            {
                Debug.WriteLine("binary file size is Zero: " + binSize.ToString() + ", " + binResSize.ToString() + ", " + data.Length.ToString());

                _albumCover.IsDownloading = false;

                return r;
            }

            if (binResSize != ((data.Length - gabStart) - gabEnd))
            {
                Debug.WriteLine("binary file size mismatch: " + binSize.ToString() + ", [" + binResSize.ToString() + ", " + (data.Length - gabStart - gabEnd) + "], " + data.Length.ToString());

                _albumCover.IsDownloading = false;

                return r;
            }

            r.WholeSize = binSize;
            r.ChunkSize = binResSize;

            byte[] resBinary = new byte[data.Length - gabStart - gabEnd];
            Array.Copy(data, gabStart, resBinary, 0, resBinary.Length);

            r.BinaryData = resBinary;

            r.IsSuccess = true;

            return r;

        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error@ParseAlbumImageData (l): " + ex.Message);
            r.IsSuccess = false;
            r.ErrorMessage = ex.Message;
            _albumCover.IsDownloading = false;
            return r;
        }
    }

    private static byte[] CombineByteArray(byte[] first, byte[] second)
    {
        byte[] ret = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        return ret;
    }

    public async Task<CommandImageResult> MpdQueryAlbumArt(string uri, bool isUsingReadpicture)
    {
        if (string.IsNullOrEmpty(uri))
        {
            CommandImageResult f = new()
            {
                ErrorMessage = "IsNullOrEmpty(uri)",
                IsSuccess = false
            };
            return f;
        }

        if (_albumCover.IsDownloading)
        {
            CommandImageResult f = new()
            {
                ErrorMessage = "_albumCover.IsDownloading",
                IsSuccess = false
            };
            return f;
        }
  
        _albumCover.SongFilePath = uri;
        _albumCover.IsDownloading = true;

        uri = Regex.Escape(uri);

        string cmd = "albumart \"" + uri + "\" 0" + "\n";
        if (isUsingReadpicture && (!string.IsNullOrEmpty(MpdVersion)))
            if (CompareVersionString(MpdVersion, "0.22.0") >= 0)
                cmd = "readpicture \"" + uri + "\" 0" + "\n";

        CommandBinaryResult result = await MpdBinarySendBinaryCommand(cmd);

        CommandImageResult b = new();
        bool r = false;

        if (result.IsSuccess)
        {
            _albumCover.BinaryData = result.BinaryData;

            if ((result.WholeSize != 0) && (result.WholeSize == result.ChunkSize))
            {
                /*
                Dispatcher.UIThread.Post(async () =>
                {

                });
                */

                if (_albumCover.BinaryData is not null)
                {
                    //_albumCover.AlbumImageSource = BitmaSourceFromByteArray(_albumCover.BinaryData);
                    b.AlbumCover.AlbumImageSource = BitmapSourceFromByteArray(_albumCover.BinaryData);
                }

                //if (_albumCover.AlbumImageSource is not null)
                if (b.AlbumCover.AlbumImageSource is not null)
                {
                    _albumCover.IsSuccess = true;
                }
                else
                {
                    Debug.WriteLine("_albumCover.IsSuccess == false; BitmaSourceFromByteArray@MpdQueryAlbumArt");
                    _albumCover.IsSuccess = false;
                }

                _albumCover.IsDownloading = false;

                r = _albumCover.IsSuccess;
            }
            else
            {
                if ((result.WholeSize != 0) && (result.WholeSize > result.ChunkSize))
                {
                    if (result.IsSuccess && (_albumCover.BinaryData is not null))
                    {
                        // TODO:
                        while ((result.WholeSize > _albumCover.BinaryData.Length) && result.IsSuccess)
                        {
                            result = await MpdReQueryAlbumArt(_albumCover.SongFilePath, _albumCover.BinaryData.Length, isUsingReadpicture);

                            if (result.IsSuccess && (result.BinaryData is not null))
                                _albumCover.BinaryData = CombineByteArray(_albumCover.BinaryData, result.BinaryData);
                            else
                                break;
                        }

                        if (result.IsSuccess)
                        {
                            /*
                            Dispatcher.UIThread.Post(async () =>
                            {

                            });
                            */
                            if (_albumCover.BinaryData is not null)
                            {
                                //_albumCover.AlbumImageSource = BitmaSourceFromByteArray(_albumCover.BinaryData);
                                b.AlbumCover.AlbumImageSource = BitmapSourceFromByteArray(_albumCover.BinaryData);
                            }

                            //if (_albumCover.AlbumImageSource is not null)
                            if (b.AlbumCover.AlbumImageSource is not null)
                            {
                                _albumCover.IsSuccess = true;
                            }
                            else
                            {
                                result.ErrorMessage = "BitmaSourceFromByteArray(_albumCover.BinaryData) returned null.";
                                Debug.WriteLine("BitmaSourceFromByteArray(_albumCover.BinaryData) returned null.");
                                _albumCover.IsSuccess = false;
                            }
                            _albumCover.IsDownloading = false;

                            r = true;
                        }
                        else
                        {
                            Debug.WriteLine("not IsSuccess @MpdQueryAlbumArt");
                            /*
                            Dispatcher.UIThread.Post(() =>
                            {
                            });
                            */
                            _albumCover.IsSuccess = false;
                            _albumCover.IsDownloading = false;
                        }
                    }
                    else
                    {

                        Debug.WriteLine("if (result.IsSuccess && (_albumCover.BinaryData is not null)) @MpdQueryAlbumArt");
                    }
                }
                else
                {

                    Debug.WriteLine("if ((result.WholeSize != 0) && (result.WholeSize > result.ChunkSize)) @MpdQueryAlbumArt");
                }
            }
        }
        else
        {
            b.IsTimeOut = result.IsTimeOut;
        }

        b.IsSuccess = r;
        if (!r)
        {
            b.ErrorMessage = result.ErrorMessage;
        }
        b.AlbumCover.SongFilePath = _albumCover.SongFilePath;
        b.AlbumCover.IsDownloading = false;
        b.AlbumCover.IsSuccess = r;
        //b.ErrorMessage = result.ErrorMessage;

        //_albumCover.AlbumImageSource = null; // Clear the image source to free memory.
        //_albumCover.IsDownloading = false;
        _albumCover = new AlbumImage(); // Reset the album cover for next use.

        return b;
    }

    private async Task<CommandBinaryResult> MpdReQueryAlbumArt(string uri, int offset, bool isUsingReadpicture)
    {
        if (string.IsNullOrEmpty(uri))
        {
            CommandBinaryResult f = new()
            {
                ErrorMessage = "IsNullOrEmpty(uri)",
                IsSuccess = false
            };
            return f;
        }

        if (!_albumCover.IsDownloading)
        {
            CommandBinaryResult f = new()
            {
                IsSuccess = false
            };
            return f;
        }

        if (_albumCover.SongFilePath != uri)
        {
            _albumCover.IsDownloading = false;

            CommandBinaryResult f = new()
            {
                IsSuccess = false
            };
            return f;
        }

        uri = Regex.Escape(uri);

        string cmd = "albumart \"" + uri + "\" " + offset.ToString() + "\n";
        if (isUsingReadpicture && (!string.IsNullOrEmpty(MpdVersion)))
            if (CompareVersionString(MpdVersion, "0.22.0") >= 0)
                cmd = "readpicture \"" + uri + "\" " + offset.ToString() + "\n";

        return await MpdBinarySendBinaryCommand(cmd);
    }

    private static int CompareVersionString(string a, string b)
    {
        return (new System.Version(a)).CompareTo(new System.Version(b));
    }

    //private static readonly System.Threading.SemaphoreSlim _semaphore = new(1, 1);

    private static Bitmap? BitmapSourceFromByteArray(byte[] buffer)
    {
        // Bug in MPD 0.23.5 
        if (buffer?.Length <= 0)
        {
            Debug.WriteLine("if (buffer?.Length > 0) @BitmapSourceFromByteArray");

            return null;
        }

        if (buffer == null)
        {
            Debug.WriteLine("buffer == null) @BitmapSourceFromByteArray");
            return null;
        }

        //await _semaphore.WaitAsync();

        using var stream = new MemoryStream(buffer);
        try
        {
            // A huge bug in Skia dll for linux. 1 out of 3 won't work on Linux. 
            //return Bitmap.DecodeToWidth(stream, 400);

            using var skbit = SKBitmap.Decode(stream);
            if (skbit is null)
            {
                return null;
            }

            // resize
            var realScale = skbit.Height < skbit.Width ? (double)skbit.Height / skbit.Width : (double)skbit.Width / skbit.Height;

            using var resized = skbit.Resize(new SKSizeI(400, (int)(realScale * 400)), SKFilterQuality.High);
            using var enc = resized?.Encode(SKEncodedImageFormat.Png, 100);
            using var stm = enc?.AsStream();
            if (stm != null)
            {
                return new Bitmap(stm);
            }
            else
            {
                return null;
            }
        }
        catch
        {
            Debug.WriteLine("Exception Bitmap.DecodeToWidth @BitmapSourceFromByteArray");

        }
        finally
        {
            //_semaphore.Release();

        }

        return null;
    }

    public void MpdBinaryConnectionDisconnect()
    {
        try
        {
            _binaryConnection.Client?.Shutdown(SocketShutdown.Both);
            _binaryConnection.Close();
        }
        catch { }
    }
}
