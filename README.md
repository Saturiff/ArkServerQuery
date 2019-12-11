# ArkServerQuery 方舟服務器查詢
專案關係圖
![專案關係_zh](/img/專案關係_zh.PNG)
![專案關係_en](/img/專案關係_en.PNG)

|專案|註解|
|:-------------:|-------------|
|ARKServerQuery|服務器查詢介面|
|ARKWatchDog|服務器監控介面|
|ServerListGen|由官方服務器列表生成可供查詢介面查詢之文字檔(ServerList.txt)|
|SourceQuery|與服務器進行Udp連線交換資料，改寫自 [brycekahle/source-query-net](https://github.com/brycekahle/source-query-net/)|


## 操作示範 & 可執行文件
[Youtube](https://youtu.be/AJW6x247SUI)  
[Ark Server Query Executable](https://github.com/reina42689/ARK-server-query-executable)  

## 撰寫環境
> Visual studio 2019  
> .Net Framework 4.7.2


## 要如何正常的偵錯/發行ARKServerQuery?
1. 建置/發行 ARKServerQuery、ARKWatchdog  
2. 在ARKServerQuery之執行檔資料夾下建立bin資料夾
3. ARKWatchDog建置目錄下的以下檔案放到./bin  
4. ServerList.txt由ServerListGen專案生成後放到./bin  
5. 最終效果應如同檔案樹  


### 檔案樹
[Ark Server Query Executable](https://github.com/reina42689/ARK-server-query-executable)  
. /  
├─bin  
│ ├─ARKWatchdog.exe  
│ ├─ICSharpCode.SharpZipLib.dll  
│ ├─ServerList.txt  
│ ├─ServerListGen.exe (optional)  
│ └─SourceQuery.dll  
├─ARKServerQuery.exe  
└─ARKServerQuery.exe.config  

  
## 最新的更新日誌 2019/12/12
新增了滾輪縮放文字的功能  


