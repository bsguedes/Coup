# Coup Bot Engine

This is an engine for the Coup Card Game. The idea is to develop bots that will play the game.

## Implementing a bot

You simply have to create a library that has a class extending from abstract class Player on CoupCore:

Please implement the following abstract methods:

### Methods that require decisions from bots
```
public abstract ActionDescriptor Play( bool mustCoup );
public abstract Card TriesToBlock( ActionType playerAction, int playerTurnIndex );
public abstract bool Challenge( int playerIndex, Card card );
public abstract Card ChooseLoseInfluence();
public abstract Card GiveCardToInquisitor( int targetIndex );
public abstract bool ShouldTargetChangeCard( int targetIndex, Card targetCard );        
public abstract Card ChooseOneToRemove( Card newCard );
```
       
### Methods to inform bots from changes in game status       
```
public abstract void SendStatus( List<PlayerDescriptor> players );
public abstract void SignalNewTurn( int playerIndex );
public abstract void SignalBlocking( int p1, ActionType actionType, int p2, Card cardBlocking );  
public abstract void SignalLostInfluence( int p, Card influence );
public abstract void SignalChallenge( int challenger, int challenged, Card card );
public abstract void SignalPlayersTargettedAction( int p1, ActionType actionType, int p2 );
public abstract void SignalPlayerAction( int p, ActionType actionType );
```
   
## Rules and more information

More info about Coup on http://boardgamegeek.com/boardgame/131357/coup
