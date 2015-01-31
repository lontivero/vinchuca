DreamBot
======

DreamBot is a resilent peer-to-peer botnet agent developed in .NET for educational purposes only. It is released with some missing features and without control panel in order to prevent misuses. However, it includes several interesting features

Goals
-----
Have some fun and explore the techniques used by popular botnet like Zeus Game Over.

+ Tested with .NET  _YES_
+ Tested with Mono  _NO_

Features
-------
* **Anti-debugging** it includes a couple of techniques to prevent debugging.
* **Single Instance** only one bot process.
* **Sandbox Detection** it doesn't run if detects a sondbox
* **Socks proxy integrated** 
* **Http proxy integrated** 
* **Https traffic tampering** it can sniff http traffic creating a fake on-the-fly certificate. 
* **Internet connection detector** it stops if no internet access
* **Peer-to-Peer protocol** the best part!!!
* **Encrypted protocol communication (RC4)**
* **Signed protocol messages** to verify botmaster sign.
* **English-like Domain Generator Algorithm** for backup channel
* **DDoS** 
   *  Http Flood
   *  Syn Flood
   *  Udp Flood
* **WebInject** (no ready but easy thanks to Http/s traffic tampering)
* **USB spreading**
* **Windows Firewall open**


Development
-----------
DreamBot was developed in vacations and is dead since then. Anyway, you are welcome to contribute code. You can send code both as a patch or a GitHub pull request. 

Build Status
------------

[![Build status](https://ci.appveyor.com/api/projects/status/dadcbt26mrlri8cg)](https://ci.appveyor.com/project/lontivero/dreambot)

