using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms; //追加
using System.Runtime.InteropServices;

class MainWindow {
  static void Main () {
    TaskbarKillForm form = new TaskbarKillForm ();
    Application.Run ();
  }
}

class TaskbarKillForm : Form {
  private Thread thread; // 定期実行するスレッド
  Int32 taskbarWnd;

  [DllImport ("shell32.dll", CallingConvention = CallingConvention.StdCall)]
  public static extern int SHAppBarMessage (ABMsg dwMessage, ref APPBARDATA pData);

  [DllImport ("user32.dll", EntryPoint = "ShowWindow")]
  public static extern int ShowWindow (Int32 hWnd, int nCmdShow);
  public const int TASKBAR_HIDE = 0;
  public const int TASKBAR_SHOW = 5;

  [DllImport ("user32.dll", EntryPoint = "FindWindow")]
  public static extern Int32 FindWindow (String lpClassName, String lpWindowName);

  public enum ABMsg : int {
    ABM_NEW = 0,
    ABM_REMOVE = 1,
    ABM_QUERYPOS = 2,
    ABM_SETPOS = 3,
    ABM_GETSTATE = 4,
    ABM_GETTASKBARPOS = 5,
    ABM_ACTIVATE = 6,
    ABM_GETAUTOHIDEBAR = 7,
    ABM_SETAUTOHIDEBAR = 8,
    ABM_WINDOWPOSCHANGED = 9,
    ABM_SETSTATE = 10
  }

  public struct APPBARDATA {
    public int cbSize;
    public IntPtr hWnd;
    public uint uCallbackMessage;
    public ABEdge uEdge;
    public RECT rc;
    public int lParam;
  }

  public enum ABEdge : int {
    ABE_LEFT = 0,
    ABE_TOP = 1,
    ABE_RIGHT = 2,
    ABE_BOTTOM = 3
  }

  [StructLayout (LayoutKind.Sequential)]
  public struct RECT {
    public int left;
    public int top;
    public int right;
    public int bottom;
  }

  // コンストラクタ
  public TaskbarKillForm () {
    // タスクバーのウィンドウハンドルを取得
    this.taskbarWnd = FindWindow ("Shell_TrayWnd", null);
    // if(this.taskbarWnd!=0){

    // }

    this.ShowInTaskbar = false;
    this.Initialize();

    this.thread = new Thread (intervalCheck); //追加
    thread.Start (); // スレッドの開始
  }

  private void TaskbarHide () {
    ShowWindow (this.taskbarWnd, TASKBAR_HIDE);

    //// タスクバーを常に表示
    APPBARDATA pData = new APPBARDATA ();
    pData.cbSize = Marshal.SizeOf (pData);
    pData.hWnd = (IntPtr) this.taskbarWnd;
    pData.lParam = (int) ABMsg.ABM_REMOVE;
    //タスクバーにメッセージ送信
    SHAppBarMessage (ABMsg.ABM_SETSTATE, ref pData);
  }

  // 終了ボタンクリック時の処理
  private void OnMenuExitClicked (object sender, EventArgs e) {
    thread.Abort (); // スレッドの停止
    Application.Exit ();
  }

  //以下追加
  private void intervalCheck () {
    while (true) {
      this.TaskbarHide ();
      System.Threading.Thread.Sleep (1000); // 1秒ごとに実行
    }
  }

  private void Initialize () {
    NotifyIcon icon = new NotifyIcon ();
    icon.Icon = new Icon (SystemIcons.Application, 40, 40);
    icon.Visible = true;
    icon.Text = "TaskbarKill";
    ContextMenuStrip menu = new ContextMenuStrip ();
    ToolStripMenuItem menuItem = new ToolStripMenuItem ();
    menuItem.Text = "&終了(&X)";
    menuItem.Click += new EventHandler (OnMenuExitClicked);
    menu.Items.Add (menuItem);

    icon.ContextMenuStrip = menu;
  }
}
