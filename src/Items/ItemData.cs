/* 
	A flat item.
*/
using System;
using System.Collections.Generic;
using Godot;

public class ItemData {
	
	public int id;
	public ItemFactory.Items itemType;
  public string json;

  public ItemData(){}

  public ItemData(ItemFactory.Items itemType, string json = ""){
    this.itemType = itemType;
    this.json = json;
  }

  public IItem Unflatten(){
    IItem item = ItemFactory.Factory(itemType, json);
    item.SetId(id);
    return item;
  }
}