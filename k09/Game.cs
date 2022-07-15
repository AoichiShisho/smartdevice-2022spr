#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲームクラス。
/// 学生が編集すべきソースコードです。
/// </summary>
public sealed class Game : GameBase
{
    // 変数の宣言
    float sec = 0;

    string pname = "t21516sn";
    string url = "";
    string str = "";

    int gameState = 0;

    const int BOX_NUM = 5;
    int[] box_x = new int [BOX_NUM];
    int[] box_y = new int [BOX_NUM];
    int[] box_speed = new int [BOX_NUM];
    int box_w = 24;
    int box_h = 24;

    int player_x = 304;
    int player_y = 400;
    int player_dir = 1;
    int player_speed = 3;

    int score = 0;
    int count = 0;

    int camera_id = 0; 
    int camera_num;
    string camera_name = ""; 
    GcCameraDevice? m_Camera;

    int active_box_num = 0;


    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame()
    {
        // キャンバスの大きさを設定します
        gc.SetResolution(640, 480);

        resetValue();
        score = gc.Random(0, 100);

        if (gc.HasUserAuthorizedPermissionCamera) {
            PlayCamera(0);
        } else
        {
            gc.RequestUserAuthorizedPermissionCameraAsync(success => {
                if (success) {
                    PlayCamera(0);
                } else {
                    camera_name = "Error: no permission";
                }
            });
        }
    }

    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {
        active_box_num = 5 + count/600;
        if (active_box_num > BOX_NUM) {
            active_box_num = BOX_NUM;
        }


        if (gc.GetPointerFrameCount(0) == 1){
            camera_id++;
            if (camera_id >= gc.UpdateCameraDevice()) {
                camera_id= 0; 
            }
            PlayCamera(camera_id);
        } 


        if (gameState == 0) {
            if (gc.GetPointerFrameCount(0) == 1) {
                gameState = 1;
            }
        } 
        else if (gameState == 1) {
            //ゲーム中の処理
            count++;
			score = count / 60;
			box_w = 24 + count / 300;
			box_h = 24 + count / 300;

			if (gc.GetPointerFrameCount(0) == 1) {
				player_dir = -player_dir;
                gc.PlaySound(GcSound.Click1);
			}

			player_x += player_dir * player_speed;

			for (int i = 0 ; i < BOX_NUM ; i++) {
				//箱を動かす処理
				box_y[i] = box_y[i] + box_speed[i];
				if(box_y[i]> 480){
					box_x[i] = gc.Random(0, 616);
					box_y[i] = -gc.Random(100, 480);
					box_speed[i] = gc.Random(3, 6);
				}
				//playerと箱の当り判定
				if (gc.CheckHitRect(player_x, player_y, 32, 32, box_x[i], box_y[i], box_w, box_h)) {
					//当たった時の処理
					gameState = 2;
				}
			}
			//playerが画面左右に着いてもgameOverになるように
			if (player_x < 0 || player_x > 608){
				gameState = 2;
			}

            gc.PlaySound(GcSound.Click2,GcSoundTrack.BGM1,true);
        } 
        else if (gameState == 2) {
            if (gc.GetPointerFrameCount(0) == 1) {
                url = "http://web.sfc.keio.ac.jp/~wadari/sdp/k07_web/score.cgi?score=" + score + "&name=" + pname;
                gc.GetOnlineTextAsync(url, out str);
			}
            gc.StopSound(GcSoundTrack.BGM1);
        }
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        // 画面を白で塗りつぶします
        gc.ClearScreen();

        gc.DrawString(str, 0, 300);

        if (gameState == 0) {
            //gameStateが0の時は"Title"と表示
            gc.DrawString("Title", 0, 400);
        } 
        else if (gameState == 1) {
            //gameStateが1の時はゲーム画面を表示
            gc.DrawCameraImage(m_Camera, player_x, player_y, 0.10f, 0.10f, 0f, true);
			for (int i=0; i<BOX_NUM; i++) {
				gc.FillRect(box_x[i], box_y[i], box_w, box_h);
			}
        }
        else if (gameState == 2) {
            //gameStateが2の時は"Game Over"と表示、スコアと通信結果も表示
            gc.DrawString("Game Over", 0, 200);
			gc.DrawString(str, 0, 300);
        }
    }

    void resetValue() {
        for(int i=0; i<BOX_NUM; i++)
        {
            box_x[i] = gc.Random(0, 616);
            box_y[i] = -gc.Random(100, 480);
            box_speed[i] = gc.Random(3, 6);
        }
    }

    void PlayCamera(int id)
    {
        if (id >= gc.UpdateCameraDevice()) {
            id = 0;
        }

        if (gc.TryGetCameraImageAll(out var devices)) {
            m_Camera = devices[id];
            camera_name = m_Camera.DeviceName;
        } else {
            camera_name = "Warn: no camera";
        }
    }
}
