using UnityEngine;

public class ToDo : MonoBehaviour
{
    /*
     * arena prefab deðiþtirilecek - rotasyon ayarlarý vs yapmak lazým
     * highlight effect lazým
     * 
     *** MainSceneManager 
     * 1) restartGame yeniden network req yapmalý
     * 3) Server testing için uncomment edilmesi gereken kýsýmlar var.
     * 6) JoinRoom'a roomCode'u parametre olarak vermek daha mý mantýklý. O durumda roomCode OnJoinRoomButtonClicked ile alýnýrdý.
     * 7) JoinRoom server'dan aldýðý dönüte göre boolean sonuç dönmeli. Sonuca göre ya panel deaktive edilir ya da panel içerisindeki Status text ile hata mesajý gösterilir.
     * 8) gameActive & isConnected baþlangýçta FALSE olmalý
     * 10) reset arena buttonu eklenmeli
     * 
     *** WebSocketBridge
     * 1) finished state i geldiðine websocket baðlantýsý kapatýlmalý
     * 2) adb logcat baþlatýldýðýnda app henüz baðlanmadan bazý outlar alýyorum. onlarý incele.
     * 
     *** GameController
     * 1) HandleGameStatusChange --> to be implemented ya da gerek var mý oyun zaten sadece biri öldüðünde bitiyor.
     *
     *** PlayerController
     * 1) ShowDamageNumber --> to be implemented
     * 2) 
     */
}
