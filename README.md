# 使用Unity实现了类魂游戏基础的玩家角色3C控制，后续功能仍在持续更新中(下方有几张gif，需等待片刻加载)

## 当前已拥有功能：

### ⭐控制器Controller： 
实现了输入和利用携程进行单击和按压的判断，使用可变参数来调整单机间隔为多少秒时为按压

![image](https://github.com/user-attachments/assets/bff37340-8424-47f3-bb88-7b287e0686e6)

### ⭐摄像机Camera：

**➡实现了摄像机固定绕玩家一定距离旋转的平滑过渡**

![无标题视频——使用Clipchamp制作](https://github.com/user-attachments/assets/4b622ed9-e20f-464a-9b3a-fb85243f0971)

**➡实现了摄像机遇到障碍自动拉近距离，离开障碍自动恢复距离**

![无标题视频——使用Clipchamp制作 (1)](https://github.com/user-attachments/assets/9af58322-d408-4331-aa5c-3b7426af9fe1)

### ⭐角色Charactor

**➡状态机预览**

![image](https://github.com/user-attachments/assets/5d87fd3d-bc5e-4dbd-ba5f-d143ae377fac)

**➡实现了基础操作、前摇后摇和指令预输入**

移动和奔跑

![无标题视频——使用Clipchamp制作 (2)](https://github.com/user-attachments/assets/6f2d6ad7-ea2a-4049-8ff6-d3f5b6abf3b1)

轻击和重击

![无标题视频——使用Clipchamp制作 (4)](https://github.com/user-attachments/assets/5c2ec108-1952-420f-8271-4fb64945849e)

奔跑后轻击和重击

![无标题视频——使用Clipchamp制作 (5)](https://github.com/user-attachments/assets/3f866a5b-c883-4d33-87ae-40aa041522d3)

翻滚和翻滚后轻击

![无标题视频——使用Clipchamp制作 (6)](https://github.com/user-attachments/assets/5e4601f1-e606-455f-99ec-5a5fcbd9d1c6)

受伤、受重伤和被击倒

![无标题视频——使用Clipchamp制作 (7)](https://github.com/user-attachments/assets/5f9d497a-3462-4cd7-a15e-ceb3f381853e)

浮空与落下

![无标题视频——使用Clipchamp制作 (8)](https://github.com/user-attachments/assets/059658e9-c878-48f5-84f4-a8a05fe2160c)

**➡实现了锁定敌人功能**

使用BoxOverlap加Mask进行检测，对碰撞到Layer为Enemy的敌人进行排序，优先锁定距离玩家最近的敌人

可通过鼠标滚轮对锁定物体进行切换

![无标题视频——使用Clipchamp制作 (3)](https://github.com/user-attachments/assets/40550f1f-c651-4388-a922-3677c09ca9ba)

**➡实现了左右手使用不同动画层级**
例如左手拳和右手剑

![无标题视频——使用Clipchamp制作 (9)](https://github.com/user-attachments/assets/ef88899e-8768-4d33-bde4-8e4cb4bba6f7)

## 动作系统实现思路

核心主要使用Unity动画系统中的动画分层功能和分层同步功能，因为黑魂的动作区分是非常明显的，本项目目前主要将想要实现的动作分为三类：

### 1、基础类：
该类动作主要体现在与世界中其他物体的交互，如使用道具、开门、爬梯子、掉落等动作。

### 2、单手绑定类动作：
该类动作主要和左手或者右手绑定，主要体现在各种攻击动作上，并且区分左手和右手

tips：因为我的前摇和后摇函数以及预输入函数都是动画回调函数，以及动作很多都使用触发器变量， 因此和当前层级无关的动作最好不要设置

### 3、单手绑定类




