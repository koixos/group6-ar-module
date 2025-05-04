using UnityEngine;

public class ToDo : MonoBehaviour
{
    /*
     * arena prefab deðiþtirilecek - rotasyon ayarlarý vs yapmak lazým
     * highlight effect lazým
     * 
     *** MainSceneManager - ARSetup - GameController scriptlerini birleþtirdim.
     * NetworkManager ve WebSocketManager scriptleri þu an active deðil.
     * 
     * 1) restartGame yeniden network req yapmalý
     * 3) Server testing için uncomment edilmesi gereken kýsýmlar var.
     * 4) GameInterface adý altýnda tüm game componentlarýný birleþtirmek mantýklý mý? Örneðin GameArena GameInterface UI'ý olarak gizlenirse vs daha mý derli toplu olur?
     * 6) JoinRoom'a roomCode'u parametre olarak vermek daha mý mantýklý. O durumda roomCode OnJoinRoomButtonClicked ile alýnýrdý.
     * 7) JoinRoom server'dan aldýðý dönüte göre boolean sonuç dönmeli. Sonuca göre ya panel deaktive edilir ya da panel içerisindeki Status text ile hata mesajý gösterilir.
     * 8) gameActive & isConnected baþlangýçta FALSE olmalý
     * 10) reset arena buttonu eklenmeli
     * 
     *** Network Manager
     * 1) OnConnectionStatusChanged
     * 
     *** PlayerSpawner
     * 2) server'dan aldýðýn dataya göre model spawnlanacak. bu data mainSceneManager'dan mý gelir yoksa nasýl olur düþün.
     * 
     *** GameController
     * 1) HandleGameStatusChange --> to be implemented ya da gerek var mý oyun zaten sadece biri öldüðünde bitiyor.
     *
     *** PlayerController
     * 1) ShowDamageNumber --> to be implemented
     * 2) 
     */
}
