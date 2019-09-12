# ArkServerQuery

## 專案
ARKServerQuery -> 服務器查詢介面，與監控介面進行交互  
ARKWatchDog -> 服務器監控介面  
ServerListGen -> 由官方服務器列表生成可供查詢介面查詢之文字檔(ServerList.txt)  
SourceQuery -> 與服務器進行Udp連線交換資料，改寫自 https://github.com/brycekahle/source-query-net/  

## 撰寫環境
> Visual studio 2019  
> .Net Framework 4.7.2

## 要如何正常的偵錯ARKServerQuery?
1. 建置 ARKServerQuery、ARKWatchdog  
2. 在ARKServerQuery之建置組態資料夾(如: debug)下建立bin資料夾
3. ARKWatchDog建置目錄下的以下檔案放到./bin  
> ARKWatchdog.exe  
> ICSharpCode.SharpZipLib.dll  
> SourceQuery.dll  
4. ServerList.txt自行生成後放到./bin  

## 要如何發行可執行軟件?
1. 發行 ARKServerQuery、ARKWatchdog  
2. 在欲存放軟件之地點新增bin資料夾  
3. ARKServerQuery發行目錄下的以下檔案放到./
> ARKServerQuery.exe
> ICSharpCode.SharpZipLib.dll  
> SourceQuery.dll  
4. ARKWatchDog發行目錄下的以下檔案放到./bin  
> ARKWatchdog.exe  
> ICSharpCode.SharpZipLib.dll  
> SourceQuery.dll  
5. ServerList.txt自行生成後放到./bin  

### 檔案樹
. /  
├─bin  
│ ├─ARKWatchdog.exe  
│ ├─ICSharpCode.SharpZipLib.dll  
│ ├─ServerList.txt  
│ └─SourceQuery.dll  
├─ARKServerQuery.exe  
├─ICSharpCode.SharpZipLib.dll  
└─SourceQuery.dll  
