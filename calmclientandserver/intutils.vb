﻿Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Text
Imports System.Security.Cryptography
Imports System.Threading

''' <summary>
''' Utilities Module {Access Internal}
''' </summary>
''' <remarks></remarks>
Friend Module intutils
    Public MainEncoding As Encoding = ASCIIEncoding.GetEncoding("windows-1255")

    Private KeyLengthBits As Integer = 256

    Private SaltLength As Integer = 8

    Private IterationCount As Integer = 2000

    Private rng As RNGCryptoServiceProvider = New RNGCryptoServiceProvider()

    Private synclockencrypt As New Object()

    Private synclockdecrypt As New Object()

    Public Function DecryptString(ciphertext As String, passphrase As String) As String
        Try
            SyncLock synclockdecrypt
                Dim expr_11 As String() = ciphertext.Split(":".ToCharArray(), 3)
                Dim iv As Byte() = Convert.FromBase64String(expr_11(0))
                Dim salt As Byte() = Convert.FromBase64String(expr_11(1))
                Dim arg_35_0 As Byte() = Convert.FromBase64String(expr_11(2))
                Dim key As Byte() = DeriveKeyFromPassphrase(passphrase, salt)
                Dim bytes As Byte() = DoCryptoOperation(arg_35_0, key, iv, False)
                Return Encoding.UTF8.GetString(bytes)
            End SyncLock
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
        End Try
        Return ""
    End Function

    Public Function EncryptString(plaintext As String, passphrase As String) As String
        Try
            SyncLock synclockencrypt
                Dim array As Byte() = GenerateRandomBytes(SaltLength)
                Dim array2 As Byte() = GenerateRandomBytes(16)
                Dim key As Byte() = DeriveKeyFromPassphrase(passphrase, array)
                Dim inArray As Byte() = DoCryptoOperation(Encoding.UTF8.GetBytes(plaintext), key, array2, True)
                Return String.Format("{0}:{1}:{2}", Convert.ToBase64String(array2), Convert.ToBase64String(array), Convert.ToBase64String(inArray))
            End SyncLock
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
        End Try
        Return ""
    End Function

    Private Function DeriveKeyFromPassphrase(passphrase As String, salt As Byte()) As Byte()
        Return New Rfc2898DeriveBytes(passphrase, salt, IterationCount).GetBytes(KeyLengthBits / 8)
    End Function

    Private Function GenerateRandomBytes(lengthBytes As Integer) As Byte()
        Dim array As Byte() = New Byte(lengthBytes - 1) {}
        rng.GetBytes(array)
        Return array
    End Function

    Private Function DoCryptoOperation(inputData As Byte(), key As Byte(), iv As Byte(), encrypt As Boolean) As Byte()
        Dim result As Byte()
        Using aesCryptoServiceProvider As AesCryptoServiceProvider = New AesCryptoServiceProvider()
            Using memoryStream As MemoryStream = New MemoryStream()
                Dim transform As ICryptoTransform = If(encrypt, aesCryptoServiceProvider.CreateEncryptor(key, iv), aesCryptoServiceProvider.CreateDecryptor(key, iv))
                Try
                    Using cryptoStream As CryptoStream = New CryptoStream(memoryStream, transform, CryptoStreamMode.Write)
                        cryptoStream.Write(inputData, 0, inputData.Length)
                    End Using
                    result = memoryStream.ToArray()
                Catch ex_5B As Exception
                    result = New Byte(-1) {}
                End Try
            End Using
        End Using
        Return result
    End Function

    Public Function JoinBytes(ByVal Original() As Byte, ByVal JoinPart() As Byte) As Byte()
        Dim JoinedBytes(Original.Length + JoinPart.Length - 1) As Byte
        Buffer.BlockCopy(Original, 0, JoinedBytes, 0, Original.Length)
        Buffer.BlockCopy(JoinPart, 0, JoinedBytes, Original.Length, JoinPart.Length)
        Return JoinedBytes
    End Function
    Public Function ChopBytes(ByVal Original() As Byte, ByVal Start As Integer, Optional ByVal Length As Integer = Nothing) As Byte()
        If Length = Nothing Or Length < 1 Then
            Length = Original.Length - Start
        End If
        Dim ChoppedBytes(Length - 1) As Byte
        Buffer.BlockCopy(Original, Start, ChoppedBytes, 0, Length)
        Return ChoppedBytes
    End Function

    Public Function validate_port(ByVal p As Integer) As Integer
        If p >= 0 And p <= 65535 Then
            Return p
        Else
            Return 100
        End If
    End Function

    Public Function createsendablebytes(ByVal ppframes As packet_frame_part()) As List(Of Byte())
        Dim toret As New List(Of Byte())
            For i As Integer = 0 To ppframes.Length - 1 Step 1
                Dim bytes() As Byte = ppframes(i)
                Dim b_l As Integer = bytes.Length
                Dim b_l_b As Byte() = Utils.Convert2Ascii(b_l)
                Dim data_byt(0) As Byte
                data_byt(0) = 1
                data_byt = JoinBytes(data_byt, b_l_b)
                Dim bts As Byte() = JoinBytes(data_byt, bytes)
                toret.Add(bts)
            Next
        Return toret
    End Function

End Module
''' <summary>
''' Utilities Module {Access public}
''' </summary>
''' <remarks></remarks>
Public Module Utils
    ''' <summary>
    ''' Converts a string to a packet.
    ''' </summary>
    ''' <param name="str">The string to convert.</param>
    ''' <returns>The converted packet.</returns>
    ''' <remarks></remarks>
    Public Function String2Packet(str As String) As Packet
        Try
            Dim returned As Packet = CType(ConvertStringToObject(str), Packet)
            Return returned
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
        End Try
        Return New Packet()
    End Function
    ''' <summary>
    ''' Converts a packet to a string.
    ''' </summary>
    ''' <param name="str">The packet to convert.</param>
    ''' <returns>The converted string.</returns>
    ''' <remarks></remarks>
    Public Function Packet2String(str As Packet) As String
        Try
            Dim returned As String = convertobjecttostring(str)
            Return returned
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
        End Try
        Return ""
    End Function
    ''' <summary>
    ''' Converts an object to a string.
    ''' </summary>
    ''' <param name="obj">The object to convert.</param>
    ''' <returns>The converted object.</returns>
    ''' <remarks></remarks>
    Public Function ConvertObjectToString(obj As Object) As String
        Try
            Dim memorysteam As New MemoryStream
            Dim formatter As New BinaryFormatter()
            formatter.Serialize(memorysteam, obj)
            Dim toreturn As String = Convert.ToBase64String(memorysteam.ToArray)
            formatter = Nothing
            memorysteam.Dispose()
            memorysteam = Nothing
            Return toreturn
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            Return ""
        End Try
    End Function
    ''' <summary>
    ''' Converts a string to an object.
    ''' </summary>
    ''' <param name="str">The string to convert.</param>
    ''' <returns>The converted string.</returns>
    ''' <remarks></remarks>
    Public Function ConvertStringToObject(str As String) As Object
        Try
            Dim memorysteam As MemoryStream = New MemoryStream(Convert.FromBase64String(str))
            Dim formatter As BinaryFormatter = New BinaryFormatter()
            Dim retobj As Object = formatter.Deserialize(memorysteam)
            formatter = Nothing
            memorysteam.Dispose()
            memorysteam = Nothing
            Return retobj
        Catch ex As ThreadAbortException
            Throw ex
        Catch ex As Exception
            Return New Object
        End Try
    End Function
    ''' <summary>
    ''' Converts a set of bytes to a string.
    ''' </summary>
    ''' <param name="bytes">The byte array to convert.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ConvertFromAscii(ByVal bytes() As Byte) As String
        Dim str As String = MainEncoding.GetString(bytes)
        Dim findnull As Integer = InStr(str, Chr(0))
        If findnull > 0 Then str = Mid(str, 1, findnull - 1)
        Return str
    End Function
    ''' <summary>
    ''' Converts a string to a set of bytes.
    ''' </summary>
    ''' <param name="str">The string to convert.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Convert2Ascii(ByVal str As String) As Byte()
        Return MainEncoding.GetBytes(str)
    End Function
End Module
