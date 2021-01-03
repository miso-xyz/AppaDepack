Imports System.Text
Imports System.IO
Imports System.IO.Compression
Imports System.Reflection
Module Module1

    Sub Main(ByVal args As String())
        Console.Title = "AppaDepack v1.0 by misonothx"
        If args.Count - 1 >= 0 Then
            If IO.File.Exists(args(0)) Then
                Console.WriteLine("Now Unpacking """ & args(0) & """...")
                Console.WriteLine()
                Unpack(args(0))
            End If
        Else
            Console.WriteLine("No input file found")
            Console.ReadKey()
        End If
    End Sub

    Private Sub CopyTo(ByVal source As Stream, ByVal destination As Stream)
        Dim array As Byte() = New Byte(81919) {}
        While True
            Dim num As Integer = source.Read(array, 0, array.Length)
            Dim count As Integer = num
            If num = 0 Then
                Exit While
            End If
            destination.Write(array, 0, count)
        End While
    End Sub
    Dim openUnpackedDir, unpackingDirectoryType, unpackingDirectoryPath
    Sub getUnpackedDirectoryType(ByVal int As Integer)
        Select Case int
            Case 1
                unpackingDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            Case 2
                unpackingDirectoryPath = AppDomain.CurrentDomain.BaseDirectory
            Case 3
                unpackingDirectoryPath = "Custom"
            Case Else
                unpackingDirectoryPath = IO.Path.GetTempPath()
        End Select
    End Sub
    Sub getCosturaCompressedFiles(ByVal filepath As String)
        Assembly.LoadFile(filepath).GetManifestResourceNames()
        For x = 0 To Assembly.LoadFile(filepath).GetManifestResourceNames().Count - 1
            If Assembly.LoadFile(filepath).GetManifestResourceNames(x).EndsWith(".compressed") Then
                Using manifestResourceStream As Stream = Assembly.LoadFile(filepath).GetManifestResourceStream(Assembly.LoadFile(filepath).GetManifestResourceNames(x))
                    Using deflateStream As DeflateStream = New DeflateStream(manifestResourceStream, CompressionMode.Decompress)
                        Using fs As New FileStream("AppaDepack/" & IO.Path.GetFileName(filepath) & "/" & Assembly.LoadFile(filepath).GetManifestResourceNames(x).Replace(".compressed", Nothing), FileMode.Create)
                            CopyTo(deflateStream, fs)
                        End Using
                    End Using
                End Using
            End If
        Next
    End Sub
    Sub WriteOriginalSettings(ByVal stringList As List(Of String))
    End Sub
    Sub Unpack(ByVal filepath As String)
        If IO.Directory.Exists("AppaDepack") = False Then
            IO.Directory.CreateDirectory("AppaDepack")
        End If
        IO.Directory.CreateDirectory("AppaDepack/" & IO.Path.GetFileName(filepath))
        getCosturaCompressedFiles(filepath)
        Dim hasSplashExe, repackable, pathtomainexe, l_args, path
        Using br As New BinaryReader(File.OpenRead(Assembly.LoadFile(filepath).Location))
            Dim bytes As Byte() = Encoding.UTF8.GetBytes("<SerGreen>")
            Dim num As Long = FindPatternPosition(br.BaseStream, bytes)
            br.BaseStream.Seek(num + bytes.LongLength, SeekOrigin.Begin)
            repackable = br.ReadBoolean()
            Console.WriteLine("Repackable: " & repackable.ToString())
            If repackable Then
                br.BaseStream.Seek(0L, SeekOrigin.Begin)
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.WriteLine("Dumping ""unpacker.exe""...")
                File.WriteAllBytes("AppaDepack/" & IO.Path.GetFileName(filepath) & "/unpacker.exe", br.ReadBytes(CInt(num)))
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("Successfully Dumped ""unpacker.exe""!...")
                br.ReadBytes(bytes.Length + 1)
                Dim count_ = br.ReadInt32()
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.WriteLine("Dumping ""packer.exe""...")
                File.WriteAllBytes("AppaDepack/" & IO.Path.GetFileName(filepath) & "/packer.exe", br.ReadBytes(count_))
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("Successfully Dumped ""packer.exe""!...")
            End If
            Console.ResetColor()
            hasSplashExe = br.ReadBoolean()
            Console.WriteLine("Has Splash Screen: " & hasSplashExe.ToString)
            If hasSplashExe Then
                Dim count_ = br.ReadInt32()
                Console.ForegroundColor = ConsoleColor.Yellow
                File.WriteAllBytes("AppaDepack/" & IO.Path.GetFileName(filepath) & "/ProgressBarSplash.exe", br.ReadBytes(count_))
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("Successfully Dumped ""ProgressBarSplash.exe""!...")
            End If
            Console.ResetColor()
            openUnpackedDir = br.ReadBoolean()
            Console.WriteLine("Open Unpacked Directory: " & openUnpackedDir.ToString)
            unpackingDirectoryType = br.ReadInt32()
            getUnpackedDirectoryType(unpackingDirectoryType)
            Console.WriteLine("Unpacked Directory: " & unpackingDirectoryPath)
            pathtomainexe = br.ReadString()
            Console.WriteLine("Path to Main EXE: " & pathtomainexe)
            l_args = br.ReadString()
            Console.WriteLine("Launch Arguments: " & l_args)
            path = br.ReadString()
            Console.WriteLine("Output path: " & path)
            Dim count__ As Integer = br.ReadInt32
            Dim bytes_ As Byte() = br.ReadBytes(count__)
            Dim prj = IO.File.CreateText("AppaDepack/" & IO.Path.GetFileName(filepath) & "/project.txt")
            prj.WriteLine("### Boolean values ###")
            prj.WriteLine("Repackable: " & repackable.ToString)
            prj.WriteLine("Has Splash EXE: " & hasSplashExe.ToString)
            prj.WriteLine("Open Unpacked Directory: " & openUnpackedDir.ToString)
            prj.WriteLine()
            prj.WriteLine("### String values ###")
            prj.WriteLine("Unpacked Directory Type: " & unpackingDirectoryType & " | " & unpackingDirectoryPath)
            prj.WriteLine("Path to Main EXE: " & pathtomainexe)
            prj.WriteLine("Launch arguments: " & l_args)
            prj.Close()
            Console.WriteLine()
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("Dumping Packed application...")
            File.WriteAllBytes("AppaDepack/" & IO.Path.GetFileName(filepath) & "/" & IO.Path.GetFileName(path), bytes_)
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("Successfully Dumped Packed Application!")
            Console.ReadKey()
        End Using
    End Sub

    Public Function FindPatternPosition(ByVal stream As Stream, ByVal byteSequence As Byte()) As Long
        If CLng(byteSequence.Length) > stream.Length Then
            Return -1L
        End If
        Dim array As Byte() = New Byte(byteSequence.Length - 1) {}
        Dim bufferedStream As BufferedStream = New BufferedStream(stream, byteSequence.Length)
        While bufferedStream.Read(array, 0, byteSequence.Length) = byteSequence.Length
            If byteSequence.SequenceEqual(array) Then
                Return bufferedStream.Position - CLng(byteSequence.Length)
            End If
            bufferedStream.Position -= CLng((byteSequence.Length - PadLeftSequence(array, byteSequence)))
        End While
        Return -1L
    End Function

    Private Function PadLeftSequence(ByVal bytes As Byte(), ByVal seqBytes As Byte()) As Integer
        Dim i As Integer
        i = 1
        While i < bytes.Length
            Dim num As Integer = bytes.Length - i
            Dim array As Byte() = New Byte(num - 1) {}
            Dim array2 As Byte() = New Byte(num - 1) {}
            System.Array.Copy(bytes, i, array, 0, num)
            System.Array.Copy(seqBytes, array2, num)
            If array.SequenceEqual(array2) Then
                Return i
            End If
            i += 1
        End While
        Return i
    End Function

End Module
