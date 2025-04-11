using UnityEngine;

public class ToDo : MonoBehaviour
{
    /*
     *** MainSceneManager - ARSetup - GameController scriptlerini birleþtirdim.
     * NetworkManager ve WebSocketManager scriptleri þu an active deðil.
     * 
     * 1) Birleþtirilmemesi daha mý doðru olur düþünülmeli.
     * 2) placeArenaButton baþlangýçta FALSE olmalý.
     * 3) Server testing için uncomment edilmesi gereken kýsýmlar var.
     * 4) GameInterface adý altýnda tüm game componentlarýný birleþtirmek mantýklý mý? Örneðin GameArena GameInterface UI'ý olarak gizlenirse vs daha mý derli toplu olur?
     * 5) Arenayý yerleþtirdikten sonra raycast'leri durdurmak mantýklý mý? Öyleyse update metodu düzenlenmeli.
     * 6) JoinRoom'a roomCode'u parametre olarak vermek daha mý mantýklý. O durumda roomCode OnJoinRoomButtonClicked ile alýnýrdý.
     * 7) JoinRoom server'dan aldýðý dönüte göre boolean sonuç dönmeli. Sonuca göre ya panel deaktive edilir ya da panel içerisindeki Status text ile hata mesajý gösterilir.
     * 8) gameActive & isConnected baþlangýçta FALSE olmalý
     * 9) OnPlaceArenaButtonClicked metodunda eðer henüz bir düzlem tespit edilmeden arena yerleþtirrilmeye çalýþýlýrsa hata msg verilmeli
     * 
     *** Network Manager
     * 1) OnConnectionStatusChanged
     * 
     *** PlayerSpawner
     * 2) server'dan aldýðýn dataya göre model spawnlanacak. bu data mainSceneManager'dan mý gelir yoksa nasýl olur düþün.
     * 
     * 
     */
}
