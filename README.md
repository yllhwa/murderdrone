# 星露谷个人战斗无人机

[English](README.en.md)

这是 [Prism-99/murderdrone](https://github.com/Prism-99/murderdrone) 的非官方
星露谷 1.6 重构版。无人机改用游戏原生 `Companion` 系统，每位玩家独立拥有、
控制和同步自己的无人机，支持本地分屏与在线联机。

## 使用

- 默认按键：键盘 `F7`，手柄按下左摇杆；可通过 GMCM 修改。
- 每位同屏玩家使用分配给自己的键盘或手柄，按键只切换自己的无人机。
- 在线联机的所有玩家都需要安装本 Mod。
- 不要与原版 Personal Combat Drone 同时安装。

## 构建

需要星露谷 1.6、SMAPI 4.x 和 .NET 6 SDK 或更新版本：

```sh
dotnet build sdv1.6.x/sdv1.6.x.csproj -c Release
```

GitHub Actions 会为每次推送和拉取请求生成可下载的测试包；推送与项目版本一致的
`v*` 标签（如 `v1.4.1`）会自动创建 GitHub Release。

建议测试双人/多人分屏、不同地图和矿井层、保存加载、装甲虫与岩石蟹。
