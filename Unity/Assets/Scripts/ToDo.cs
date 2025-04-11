using UnityEngine;

public class ToDo : MonoBehaviour
{
    /*
     *** MainSceneManager - ARSetup - GameController scriptlerini birle�tirdim.
     * NetworkManager ve WebSocketManager scriptleri �u an active de�il.
     * 
     * 1) Birle�tirilmemesi daha m� do�ru olur d���n�lmeli.
     * 2) placeArenaButton ba�lang��ta FALSE olmal�.
     * 3) Server testing i�in uncomment edilmesi gereken k�s�mlar var.
     * 4) GameInterface ad� alt�nda t�m game componentlar�n� birle�tirmek mant�kl� m�? �rne�in GameArena GameInterface UI'� olarak gizlenirse vs daha m� derli toplu olur?
     * 5) Arenay� yerle�tirdikten sonra raycast'leri durdurmak mant�kl� m�? �yleyse update metodu d�zenlenmeli.
     * 6) JoinRoom'a roomCode'u parametre olarak vermek daha m� mant�kl�. O durumda roomCode OnJoinRoomButtonClicked ile al�n�rd�.
     * 7) JoinRoom server'dan ald��� d�n�te g�re boolean sonu� d�nmeli. Sonuca g�re ya panel deaktive edilir ya da panel i�erisindeki Status text ile hata mesaj� g�sterilir.
     * 8) gameActive & isConnected ba�lang��ta FALSE olmal�
     * 9) OnPlaceArenaButtonClicked metodunda e�er hen�z bir d�zlem tespit edilmeden arena yerle�tirrilmeye �al���l�rsa hata msg verilmeli
     * 
     *** Network Manager
     * 1) OnConnectionStatusChanged
     * 
     *** PlayerSpawner
     * 2) server'dan ald���n dataya g�re model spawnlanacak. bu data mainSceneManager'dan m� gelir yoksa nas�l olur d���n.
     * 
     * 
     */
}
