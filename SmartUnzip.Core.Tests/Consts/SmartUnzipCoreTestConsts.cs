using System.Collections.Generic;

namespace SmartUnzip.Core.Tests.Consts;

public class SmartUnzipCoreTestConsts
{

    public const string DefaultPassword = "123";

    /// <summary>
    ///  测试文件夹
    /// </summary>
    public const string TestDirectory = @"TestZips";

    /// <summary>
    /// 测试文件列表
    /// </summary>
    public static readonly List<string> TestFiles =
    [
        @$"{TestDirectory}/test.zip",
        @$"{TestDirectory}/test-password-123.7z",
        @$"{TestDirectory}/Parts/7z.7z.001",
        @$"{TestDirectory}/Parts/7z.7z.002",
        @$"{TestDirectory}/Parts/7z.7z.003",
        @$"{TestDirectory}/Parts-Password/7z-password-123.7z.001",
        @$"{TestDirectory}/Parts-Password/7z-password-123.7z.002",
        @$"{TestDirectory}/Parts-Password/7z-password-123.7z.003",
        @$"{TestDirectory}/Parts-Password/7z-password-123.7z.004",
        @$"{TestDirectory}/Parts-Password/7z-password-123.7z.005",
    ];
}