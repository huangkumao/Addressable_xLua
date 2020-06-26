# Addressable_xLua
使用Addressable热更新资源和Lua脚本

在游戏启动的时候检查资源获取更新资源大小 并下载

1: 设置远程加载地址

![Image text](https://github.com/huangkumao/GitProjectImgs/blob/master/AB/ab1.png)

2:选择Disable Catalog Update On Startup 禁用启动时自动更新Catalog
  选择Build Remote Catalog 会生成远程使用的Catalog 并为其选择一个远程地址

![Image text](https://github.com/huangkumao/GitProjectImgs/blob/master/AB/ab2.png)

3: 第一次生成AB资源包 New Build

![Image text](https://github.com/huangkumao/GitProjectImgs/blob/master/AB/ab3.png)

4: 资源变更以后 选择Update a Pervious Build 更新上次一生成的AB和Catalog文件

![Image text](https://github.com/huangkumao/GitProjectImgs/blob/master/AB/ab4.png)

5: 选择上次生成的bin文件 生成的新的资源和Catalog拷贝到服务器 启动后便可以自动识别并更新

![Image text](https://github.com/huangkumao/GitProjectImgs/blob/master/AB/ab5.png)
