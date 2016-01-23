/************************************************
AndroidBuild.cs

Copyright (c) 2016 LotosLabo

This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

/* Androidにapkファイルをインストールするクラス. */
public class AndroidBuild : EditorWindow {

	/// <summary>
	/// ボタン、固定テキスト名.
	/// </summary>
	private const string ADB_LABEL_NAME = "android_adb";
	private const string SELECT_LABEL_NAME = "apk_file";
	private const string SELECT_BTN_NAME = "Select";
	private const string BUILD_BTN_NAME = "Build";
	private const string CANCEL_BTN_NAME = "Cancel";
	private const string LIST_CLEAR_BTN_NAME = "List Clear";
	private const string INSTALL_COMMAND = "install -r ";
	private const string COMPARE_WINDOWS_ADBNAME = "adb.exe";
	private const string COMPARE_MAC_ADBNAME = "adb";
	private const string ADB_SELECT_WARNING = "Please Select the adb files.";
	private const string WAITING_FOR_DEVICE = "- waiting for device -";
	private const string SCROLLVIEW_NAME = "Log";
	private const string LISTVIEW_DEFAULT_TEXT = "<Log List>";
	private const string NEXT_LINE_CODE = "\n";
	
	/// <summary>
	/// ボタンのサイズ.
	/// </summary>
	private Vector2 m_SelectBtnSize = new Vector2(100,20);
	private Vector2 m_ListClearBtnSize = new Vector2(80,20);
	private Vector2 m_BulidBtnSize = new Vector2(150,40);
        private Vector2 m_CancelBtnSize = new Vector2(150, 40);

	/// <summary>
	/// ファイルパス欄.
	/// </summary>
	private string m_labelName = string.Empty;

	/// <summary>
	/// ファイルパス、ファイル名.
	/// </summary>
	private string m_filePath = string.Empty;
	private string m_fileName = string.Empty;

	/// <summary>
	/// Projectルートパス.
	/// </summary>
	private string m_parentPath = string.Empty;

	/// <summary>
	/// AndroidADBルートパス.
	/// </summary>
	private string m_adbPath = string.Empty;

	/// <summary>
	/// EditorPrefs用キー.
	/// </summary>
	private const string ADB_KEY = "ANDROID_ADB_PATH";

	/// <summary>
	/// adb保存用path.
	/// </summary>
	private string adbPath {
		set { EditorPrefs.SetString(ADB_KEY, value); }
		get { return EditorPrefs.GetString(ADB_KEY, m_adbPath); }
	}

	/// <summary>
	/// AndroidADBファイル選択ディレクトリパス.
	/// </summary>
	private const string ANDROID_ROOT_PATH = "/Applications/";

	/// <summary>
	/// ファイル選択ボタンフラグ.
	/// </summary>
	private bool is_select_Btn = true;

	/// <summary>
	/// apkファイル選択フラグ.
	/// </summary>
	private bool is_apk_file = false;

	/// <summary>
	/// adbファイル選択フラグ.
	/// </summary>
	private bool is_adb_file = false;

	/// <summary>
	/// スクロールビューポジション.
	/// </summary>
	private Vector2 scrollView_Position;

	/// <summary>
	/// スクロールビューサイズ.
	/// </summary>
	private Vector2 scrollView_Size = new Vector2(500,100);

	/// <summary>
	/// リストビューテキスト.
	/// </summary>
	private string listview_text = LISTVIEW_DEFAULT_TEXT;

	/// <summary>
	/// 2度呼び出し禁止フラグ.
	/// </summary>
	private bool is_done = false;

	/// <summary>
	/// プロセス.
	/// </summary>
	private System.Diagnostics.Process m_process = null;

	/// <summary>
	/// ボタンアクティブフラグ.
	/// </summary>
	private bool is_btn_active = false;

	/// <summary>
	/// Editor追加.
	/// </summary>
	[MenuItem("AndroidBuild/Build")]
	public static void ShowWindow() {
		var window = EditorWindow.GetWindow(typeof(AndroidBuild));

		// 常に手前に表示.
		window.ShowUtility();
		
		// ウインドウのサイズ.
		window.minSize = new Vector2(500,300);
		window.maxSize = new Vector2(500,300);
	}

	// 初期化.
	void OnEnable() {
		m_labelName = string.Empty;
		m_filePath = string.Empty;
		m_fileName = string.Empty;
		m_parentPath = string.Empty;
		is_select_Btn = true;
		is_apk_file = false;
		is_adb_file = false;
		m_process = null;
		is_btn_active = false;

		// adbPathが存在するときは、prefsから読み込む.
		if(EditorPrefs.HasKey(ADB_KEY)) {
			m_adbPath = adbPath;
			is_adb_file = true;
		}
	}

	void OnGUI() {
		// スタイルの作成.
		GUIStyle guiStyle = new GUIStyle();

		// テキストカラーに黄色を指定.
		guiStyle.normal.textColor = Color.yellow;

		// スペース.
		EditorGUILayout.Space();

		// adbのパス表示.
		EditorGUILayout.LabelField(ADB_LABEL_NAME, m_adbPath, guiStyle);

		// ボタンの非アクティブ化.
		EditorGUI.BeginDisabledGroup (is_btn_active);

			// adbファイル選択ボタン.
			if(GUILayout.Button(SELECT_BTN_NAME, GUILayout.Width(m_SelectBtnSize.x), GUILayout.Height(m_SelectBtnSize.y))) {
				if(is_select_Btn) {
					// ファイルブラウザを開く.
					string filePath = EditorUtility.OpenFilePanel (
										"Select adb",
										ANDROID_ROOT_PATH,
										""
									    );

					if(filePath.Length <= 0) return;

					// ファイル名を取得.
					string filePathName = System.IO.Path.GetFileName(filePath);

					string adbName = string.Empty;

					// EditorがWindowsの時.
					#if UNITY_EDITOR_WIN
						adbName = COMPARE_WINDOWS_ADBNAME;
					#endif

					// EditorがMacの時.
					#if UNITY_EDITOR_OSX
						adbName = COMPARE_MAC_ADBNAME;
					#endif

					// 選択されたファイルが"adb"であるか.
					if(filePathName != adbName) {
						m_adbPath = ADB_SELECT_WARNING;
						is_adb_file = false;
						return;
					}

					m_adbPath = filePath;
					is_adb_file = true;
				}
			}

		EditorGUI.EndDisabledGroup ();

		EditorGUILayout.Space();

		// ファイルパス表示のラベル.
		EditorGUILayout.LabelField(SELECT_LABEL_NAME, m_labelName, guiStyle);

		EditorGUI.BeginDisabledGroup (is_btn_active);

			// ファイル選択ボタン.
			if(GUILayout.Button(SELECT_BTN_NAME,GUILayout.Width(m_SelectBtnSize.x),GUILayout.Height(m_SelectBtnSize.y))) {
				if(is_select_Btn) {
					// Projectのルートディレクトリ取得.
					m_parentPath = System.IO.Directory.GetParent(Application.dataPath).ToString();

					string filePath = EditorUtility.OpenFilePanel (
									    "Select File",
										    m_parentPath,
										    "apk"
								        );

                    if (filePath.Length <= 0) return;

					m_filePath = filePath;

					// ラベルにファイル名を入れる.
					m_fileName = System.IO.Path.GetFileName(m_filePath);
					m_labelName = m_fileName;

					is_apk_file = true;
				}
			}

		EditorGUI.EndDisabledGroup ();
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		// 横並びにする.
		EditorGUILayout.BeginHorizontal();

			// スクロールビューラベル名表示.
			EditorGUILayout.LabelField(SCROLLVIEW_NAME);

			EditorGUI.BeginDisabledGroup (is_btn_active);

				// リストクリアボタン.
				if( GUILayout.Button (LIST_CLEAR_BTN_NAME, GUILayout.Width(m_ListClearBtnSize.x),GUILayout.Height(m_ListClearBtnSize.y))) {
					if(!string.IsNullOrEmpty(listview_text)) {
						listview_text = LISTVIEW_DEFAULT_TEXT;
					}
				}

			EditorGUI.EndDisabledGroup ();

		EditorGUILayout.EndHorizontal();


		// スクロールビューの作成.
		scrollView_Position = EditorGUILayout.BeginScrollView(scrollView_Position, GUI.skin.box, GUILayout.Width (scrollView_Size.x), GUILayout.Height (scrollView_Size.y));
				
				if(!string.IsNullOrEmpty(listview_text)) {
					// スクロールビューラベル.
					GUILayout.Label(listview_text);
				}

		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();
	
		EditorGUILayout.BeginHorizontal();

			EditorGUILayout.Space();

			// キャンセル.
            if (GUILayout.Button(CANCEL_BTN_NAME, GUILayout.Width(m_CancelBtnSize.x), GUILayout.Height(m_CancelBtnSize.y))) {
					if(!is_select_Btn && is_adb_file && is_apk_file) {
						// プロセスが終了していない時.
						if(!m_process.HasExited) {
							is_done = true;
							Process_Exit(m_process, null);
						}
					}
				}

			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup (is_btn_active);

				// ビルドボタン.
				if( GUILayout.Button (BUILD_BTN_NAME,GUILayout.Width(m_BulidBtnSize.x),GUILayout.Height(m_BulidBtnSize.y))) {
					// adbファイルとapkファイルが選択されている時.
					if(is_select_Btn && is_adb_file && is_apk_file) {
						Process_Start();
					}
				}

			EditorGUI.EndDisabledGroup ();

			EditorGUILayout.Space();

		EditorGUILayout.EndHorizontal();
	}

	private void Process_Start() {
		// プロセス作成.
		System.Diagnostics.Process process = new System.Diagnostics.Process();

		// adbのPathを配置.
		process.StartInfo.FileName = m_adbPath;

		// プロセス起動にシェルを使用するかどうか.
		process.StartInfo.UseShellExecute = false;

		// 出力を読み取り可.
		process.StartInfo.RedirectStandardOutput = true;

		// プロセス出力イベント設定.
		process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(OutputHandler);

		// エラー出力読み取り可.
		process.StartInfo.RedirectStandardError = true;

		// エラー出力イベント設定.
		process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ErrorOutputHanlder);

		// 入力を読み取り不可.
		process.StartInfo.RedirectStandardInput = false;

		// 新しいウインドウを作成しない.
		process.StartInfo.CreateNoWindow = true;

		// インストールコマンド指定.
		process.StartInfo.Arguments = INSTALL_COMMAND +  m_filePath;

		// プロセス終了時にExitedイベントを発生.
		process.EnableRaisingEvents = true;

		// プロセス終了時に呼び出されるイベントの設定.
		process.Exited += new System.EventHandler(Process_Exit);

		// プロセスの保持.
		m_process = process;

		// プロセスの起動.
		process.Start();

		listview_text += NEXT_LINE_CODE + "---------------------------------- Process Start ----------------------------------";
		
		// スクロールバーの位置を最下部まで送る.
		scrollView_Position.y = Mathf.Infinity;

		// プロセス結果出力.
		process.BeginOutputReadLine();

		// プロセスエラー結果出力.
      process.BeginErrorReadLine();

		is_select_Btn = false;

		is_btn_active = true;

		is_done = true;

		// adbPathを保存.
		adbPath = m_adbPath;		
	}

	// 標準出力時.
	private void OutputHandler(object sender, System.Diagnostics.DataReceivedEventArgs args) {
		if(!string.IsNullOrEmpty(args.Data)) {
			// リストビューテキストに追加.
			listview_text += NEXT_LINE_CODE + args.Data;
			scrollView_Position.y = Mathf.Infinity;
		}
	}
	
	// エラー出力時.
	private void ErrorOutputHanlder(object sender, System.Diagnostics.DataReceivedEventArgs args) {
		if(!string.IsNullOrEmpty(args.Data)) {
			listview_text += NEXT_LINE_CODE + args.Data;
			scrollView_Position.y = Mathf.Infinity;
		}

		// 端末が接続されていない場合はプロセスを閉じる.
		if(args.Data.Contains(WAITING_FOR_DEVICE)) {
			is_done = true;
			Process_Exit(sender,null);
		}
	}

	// プロセス終了時.
	private void Process_Exit(object sender, System.EventArgs e) {
		// 2度呼び出し禁止.
		if(is_done) {
			System.Diagnostics.Process proc = (System.Diagnostics.Process)sender;

			is_select_Btn = true;

			is_btn_active = false;

			listview_text += NEXT_LINE_CODE + "---------------------------------- Process End -----------------------------------";
			scrollView_Position.y = Mathf.Infinity;

			is_done = false;

			// プロセスを閉じる.
			proc.Kill();
		}
	}
}
