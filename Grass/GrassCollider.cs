using UnityEngine;
using System.Collections;

public class GrassCollider : MonoBehaviour {		//used to color the grass red when in collision
		
	public tk2dSprite[] singleTiles;				//the tiled or single tiles that compose the grass
	public tk2dTiledSprite[] tiledTiles;
	public int collisionCounter = 0;				//keeps track of how many other grass patches this one is overlapping

	public bool 
		isMoving = true,	
		inCollision = false;		

	private Color myGreen;

	void Start () 
	{
		myGreen = ((tk2dSprite)singleTiles[0]).color;	
	}

	void OnCollisionEnter(Collision col)
	{
		if(isMoving && col.gameObject.layer == LayerMask.NameToLayer("Grass"))//isMoving prevents collided static tiles from becoming red too when we slide the selected building on top
		{	
			collisionCounter++;			
			
			foreach (tk2dSprite tile in singleTiles) 
			{				
				tile.color = Color.red;			
			}
			
			foreach (tk2dTiledSprite tile in tiledTiles) 
			{
				tile.color = Color.red;	
			}

			inCollision = true;			
		}
	}
	
	void OnCollisionExit(Collision col)
	{
		if(isMoving && col.gameObject.layer == LayerMask.NameToLayer("Grass"))
		{
			collisionCounter--;	
						
			if(collisionCounter == 0)
			{				
				foreach (tk2dSprite tile in singleTiles) 
				{				
					tile.color = myGreen;		
				}
				
				foreach (tk2dTiledSprite tile in tiledTiles) 
				{
					tile.color = myGreen;
				}
				
				inCollision = false;
			}
		}
	}
	
}
