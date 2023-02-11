# CW_Addon_Template

目前就这么简易，自行发挥，文档后续补充

简易版：

## 一切开始之前

修改mod.json文件, 修改Main.cs中的命名空间以及主类名称

## CW_Addon : MonoBehaviour

### 字段

|字段名|类型|描述|
|:----|:----:|:----:|
|this_mod|__Mod|对NCMS提供的[Mod](https://denq04.github.io/ncms/docs/types/mod/)的浅拷贝，成员与之相同|
|name|string|该附属名称中最后一个'.'后的所有内容|

### 方法

|方法|返回|描述|
|:----|:----:|:----:|
|Log(string, object[])|void|带附属名称的输出|
|load_mod_info(Type)|void|初始化附属信息，并向核心传递该附属信息|
