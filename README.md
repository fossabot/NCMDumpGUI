# NCMDumpGUI

> [!CAUTION]
> 此应用只用于学习用途，下载后请在24小时内删除，禁止用于商业或违法用途！请在遵守 NCM 文件提供平台的服务条款下使用本应用，作者对商业或违法使用本软件造成的任何后果不承担任何责任！
>
> 使用本软件转换的音频请在不侵犯著作权的前提下使用。如需商业，请从平台或版权所有人购买对应歌曲

这是一个简单程序，使用到了 `ncmdump` 项目实现 ncm 解密功能

适用于对命令行一问三不知的小白用户

本项目使用 .NET 8 和 C# WinForm 编写，主体代码为 `ncmdump` 的示例，本项目只是写了一个用户界面

## 特点

- 小体积
- 低内存占用
- 功能强悍
- 开箱即用

## 下载

请在 [Releases](https://github.com/WhatDamon/NCMDumpGUI/releases) 页面下载最新版本

注意请提前安装好 .NET 8 运行时，点击[此处](https://windows.net)前往官网

仅适用于 Windows 平台，系统支持详见[此处](https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md#Windows)

如果您正在使用 Windows 不受支持，可以修改项目属性 .NET 版本为 .NET 6 以获得对 Windows 7、8.1 等版本的支持

当然你也可以打包一份含有全部依赖的版本，也可以在 Windows 7 这类老系统运行，除了会非常占用空间外

## TODO

- [x] 基本用户界面
- [x] 异常处理
- [x] 批量处理
- [x] 播放
- [ ] 窗口置顶
- [ ] 支持使用 `ncmdump.exe`
- [ ] 更详细的状态信息
- [ ] 导出位置选择
- [ ] 其他格式加密音频的支持

## 截图

_截图来自 `v1.0.1.2`_

![在 Windows 11 上运行](/screenshots/Windows11.png)
![在 Windows 7 上运行](/screenshots/Windows7.png)

## 依赖

- [ncmdump](https://github.com/taurusxin/ncmdump)（MIT 许可证）
	+ [taglib](https://github.com/taglib/taglib)（LGPL 2.1、MPL 1.1 许可证）
- [NAudio](https://github.com/naudio/NAudio)（MIT 许可证）
- [Costura.Fody](https://github.com/Fody/Costura)（MIT 许可证）

## 鸣谢

本项目使用到了 https://github.com/taurusxin/ncmdump 项目中的 `libncmdump.dll`