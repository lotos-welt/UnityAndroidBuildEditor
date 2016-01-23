# 概要
Unityで使用できるAndroidデバイスにAPKファイルをインストールできるEditor拡張です。  
　　
# 使用方法
EditorディレクトリごとAssets以下に配置してください。  
Unityのメニューバーに「AndroidBuild」の項目が追加されるので、  
クリックして「Build」を選択してください。  

![UnityAndroidBuildTool](http://i.imgur.com/lolItqu.jpg "UnityAndroidBuildTool")  

android_adbの「Select」ボタンを押して、AndroidSDKのadb.exeを選択してください。  
一度選択するとパスが保存されますので次から選択しなくても大丈夫です。  

次にapk_fileの「Select」ボタンを押して、APKファイルを選択してください。  

次にAndroidデバイスをコンピュータと接続してください。  
接続後に「Build」ボタンを押すとAndroidデバイスにアプリケーションがインストールされます。  
Log欄にてエラーが出た場合はもう一度接続方法を確認してください。  
　　
# ライセンス  
This software is released under the MIT License, see LICENSE.txt.
