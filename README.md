# ArkServerQuery 方舟伺服器查詢
### Screenshot
![Executing screenshot](/img/program_execute_window.png)  

### Solution information
![Solution description image](/img/ArkServerQueryProjectGraph.png)  

|專案|註解|
|:-------------:|-------------|
|ARKServerQuery|伺服器查詢、監控介面|
|ServerListGen|由官方伺服器列表生成可供查詢介面查詢之文字檔(ServerList.txt)|
|SourceQuery|與伺服器進行Udp連線交換資料，改寫自 [brycekahle/source-query-net](https://github.com/brycekahle/source-query-net/)|


## 操作示範 & 可執行文件
[Youtube](https://youtu.be/AJW6x247SUI)  
[Ark Server Query Executable](https://github.com/reina42689/ARK-server-query-executable)  

## 注意事項
1. 該軟體預設為至頂視窗(OnTop)
2. 該軟體無法在一般模式下被拖曳、縮放

## 快捷功能
按住鍵盤左上角的 " ` " 或 " ~ " 鍵，並將滑鼠懸停在人數監控介面的文字上時，可以使用下列功能
|動作|行為|
|---|---|
|滑鼠左鍵點擊|拖曳|
|滑鼠使用滾輪|縮放|

## 撰寫環境
> Visual studio 2019  
> .Net Framework 4.7.2


## 要如何正常的偵錯/發行ARKServerQuery?
1. 建置/發行 ARKServerQuery  
2. 在ARKServerQuery之執行檔資料夾下建立bin資料夾  
3. ServerList.txt由ServerListGen專案生成後放到./bin  
4. 最終效果應如同檔案樹  


### 檔案樹
[Ark Server Query Executable](https://github.com/reina42689/ARK-server-query-executable)  
. /  
├─bin  
│ ├─ICSharpCode.SharpZipLib.dll  
│ ├─ServerList.txt  
│ ├─ServerListGen.exe (optional)  
│ └─SourceQuery.dll  
├─ARKServerQuery.exe  
└─ARKServerQuery.exe.config  

  
## 最新的更新日誌 2020/04/30
將監控介面專案(Watchdog)併入查詢介面(ARKServerQuery)  
新增快捷鍵縮放監控介面  
補充快捷鍵的詳細操作方式  
註解、程式碼用語統一與修正

