# Quickstarts：RimWorld 开发者快速测试场景

跳过所有菜单，直接进入一个已经按你的需要配置好的殖民地。Quickstarts 把 RimWorld 的开发者快速测试按钮换成你自己的场景，用 C# 写，从开发者菜单或命令行参数启动。不用选场景，不用生成星球，不用反复刷小人，也不用挑降落点。

```csharp
public class TinyColony : AbstractQuickstart {
  public override TaggedString description => "One small map, three colonists, paused.";

  public override int mapSize => 50;

  public override void PrepareColonists(List<Pawn> pawns) {
    foreach (Pawn pawn in pawns) {
      pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
    }
  }
}
```

## 这是给谁用的

模组作者。Quickstarts 对普通游戏没有任何影响，所以除非你在做模组或测试模组，没有理由订阅它。

## 和原版的快速测试有什么区别

原版的快速测试按钮把你丢到一张随机地图上，配一批随机小人，两样都改不了。每次启动都是不同的殖民地，所以同一件事没法测第二遍。

Quickstarts 把那个按钮换成一个列表，列出所有已加载模组定义的快速场景。选一个，你就已经站在地图上了，游戏暂停，殖民地也配置好了。

固定世界种子，两次运行就会得到同一颗星球、同一个降落地块和同一批殖民者。出问题可以重放，而不是一次性消失。

每个非抽象子类都会自动出现在列表里。加一个快速场景，只需要写一个类。

## 地图加载前能准备些什么

钩子会在关键节点触发：生成之前、世界创建之后、场景完成自身初始化之后，以及殖民者生成之后。你可以设定小人数量、直接完成研究、发放装备，或者打开你正在改的那个面板。

## 能当作 CI 的冒烟测试跑吗

可以，这正是写它的理由。带上校验参数跑同一个场景，游戏会对着运行中的模拟逐条断言，然后以成功或失败的退出码结束。它同时输出 JSON 报告和 JUnit XML，所以 CI 可以把失败的断言直接标注到拉取请求上。不需要显示器。

默认情况下，游戏日志里的红色报错会让这次运行判定为失败，这正是用来抓模组冲突的。真实时间的超时会中断卡住的运行，并指出它死在哪个阶段。

## 需求

RimWorld 1.5、1.6 或 1.7，开启开发者模式，以及一个补丁库。Harmony 和 Concord 都可以。两个都有时，Quickstarts 会选 Concord。

## 如何开始

引用 RimWorks.Quickstarts.Ref 包，继承 AbstractQuickstart，你的快速场景就会出现在列表里。如果想从模组里带上一个，又不想把这个依赖强加给玩家，就把它放进由 loadFolders.xml 控制的文件夹。

源码与文档：https://github.com/RimWorks/Rimworld-Quickstarts

## RimWorks 的其他模组开发工具

- [Pickle](https://steamcommunity.com/sharedfiles/filedetails/?id=3791648678)：对运行中的 RimWorld 会话跑 Gherkin 测试。
- [RimLogging](https://steamcommunity.com/sharedfiles/filedetails/?id=3733484696)：结构化日志查看器，一键分享错误报告。
