# Unity HighKick : A [FacePunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks) implementation

## What is it ?

It s a simple Steam multiplayer implementation for Unity. It handle message and components synchronisation between multiple players.

I did it because there are only few examples on how to implement multiplayer with Steamworks

Warning : this project is a personal project and it can evolve (or not)

## How we use

The way to prepare the components and the multiplayer code is greatly inspired by [Photon](https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/player-networking). 
You can see some examples in the ``Example`` Folder

### Synchronise a component and it positions

* Add a ``HView``, a ``HTransformView``

* Drag the HTransformView to the HView observed components

### Only one prefab represent your player

An important aspect of the game is that only your script is handled. For example if we have 3 differents players in the game, you want only 1 player to follow your input

In your script add a   view 
```csharp
    private HView view;
    void Start(){
        view = GetComponent<HView>();
    }
```

in the update, only your script is handled
```csharp
    void Update(){
	    if (!view.isMine){return;}
	    // My functions like : Move();
    }
```

### Create your own script component:

if you want a component that share the same values with others player, you can inherit : MultiplayerBehavior
(think of adding it to HView observed components)

```csharp

	public class MyPlayer : MultiplayerBehavior{
        public int health;

        public override void OnReadingView(ViewMessage obs)
        {
            if (view.isMine)
            {
                return;
            }
            health = (float)obs.ReceiveValue("health");
        }

        public override void OnWritingView(ViewMessage obs)
        {
            if (view.isMine)//send only your health
            {
                obs.SendValue("health", health);
            }
        }
}
```

### Send your own message

Another functionnality is to send a custom message to other players. For example we want to send to all the players a notification for pausing the game.

* Open ``MessageType.cs`` and add ``START_PAUSE`` in the enum
* In your pause function send a message to all the players
``

```csharp
    public void WantPause()
    {
        Message m = new Message(MessageType.START_PAUSE, "");
        P2PSend.SendPacket(m);
    }
```

* Listen to Pause message 
```csharp
	public class PauseListener : MonoBehavior, IListener{
		void Start(){
			//We add the Pause Listener
		    MessageHandlerNetwork.AddListener(MessageType.START_PAUSE, this);
		}
	
        //We receive a message notification
        public void OnNotify(Message message)
        {
            switch (message.MESSAGE_TYPE)
            {
                case MessageType.START_PAUSE://if it's a pause
                    Debug.Log("WE SET THE GAME IN PAUSE");
                    //....
                    break;
            }
        }
	}
```


## Possible improvment

- Clean player Id struct
- Fix non serializable Unity struct (Vector, Quaternion)
- Add more View components (RigidbodyView etc ...)