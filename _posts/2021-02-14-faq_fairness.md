---
layout: post
title: "对每个元素抽取是公平的吗"
description: ""
categories: [快速问答]
tags: [使用过程,概率] 
redirect_from:
  - /2021/02/14/
---

可以说是公平的<br/>
<br/>
现代计算机生成的随机数依赖时间，计算机系统的当前运算承载量，计算机设备的熵值等自然随机事件得出[1]<br/>
由[大数定理](https://baike.baidu.com/item/%E5%A4%A7%E6%95%B0%E5%AE%9A%E5%BE%8B)可以说生成随机数是具有相对较强的公平性和随机性的<br/>
<br/>
Let's Pick UWP 在抽取每一个元素时采用了计算机系统的随机数，抽取每一个元素的概率可以看作是相同的，故抽取是公平的<br/>