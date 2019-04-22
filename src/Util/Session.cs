/*
  This pseudo-singleton is the focal point for the active session's state.
  The Session should be the root of the scene in the game. If it's null, things simply
  won't work.

*/
using Godot;
using System;
using System.Collections.Generic;
using System.Text;


public class Session : Node {
  public static Session session;
  
  public Career career;
  public NetworkSession netSes;
  public Random random;
  public AudioStreamPlayer jukeBox;
  public Vector2 mousePosition;
  public Vector2 mouseMovement;
  public Sound.Songs currentSong;

  public Node activeMenu;
  public Node activeGamemode;

  // Settings
  public float masterVolume, sfxVolume, musicVolume;
  public string userName;
  public float mouseSensitivityX, mouseSensitivityY;
  public DeviceManager.Devices player1Device;

  // Input
  List<DeviceState> deviceStates;
  

  public static int NextItemId(){
    return 0;
  }

  public static int NextActorId(){
    return 0;
  }

  public override void _Ready() {
    EnforceSingleton();
    ChangeMenu(Menu.Menus.Main);
    InitJukeBox();
    InitSettings();
    PerformTests();

    // REMOVE BELOW THIS LINE
    AddDevice(0);

    List<InputMapping> mappings = new List<InputMapping>();
    for(int i = 0; i< 200; i++){
      mappings.Add(new InputMapping(
        InputMapping.Inputs.KeyboardKey,
        i,
        i
      ));
    }

    DeviceState device = deviceStates[0];

    source = new MappedInputSource(device, mappings);
    handler = new DebugInputHandler();
    handler.RegisterInputSource(source);
  }

  public MappedInputSource source;
  public DebugInputHandler handler;

  public override void _Process(float delta){
    // REMOVE BELOW THIS LINE
    handler.Update(delta);
  }

  public override void _Input(Godot.InputEvent evt){
    InputEventMouseMotion mot = evt as InputEventMouseMotion;
    if(mot != null){
      mousePosition = mot.GlobalPosition;
      mouseMovement = mot.Relative;
    }
  }

  public void PerformTests(){
    Test.Init();
  }

  public void InitSettings(){
    SettingsDb db = new SettingsDb();
    masterVolume = Util.ToFloat(db.SelectSetting("master_volume"));
    sfxVolume = Util.ToFloat(db.SelectSetting("sfx_volume"));
    musicVolume = Util.ToFloat(db.SelectSetting("music_volume"));
    userName = db.SelectSetting("username");
    mouseSensitivityX = Util.ToFloat(db.SelectSetting("mouse_sensitivity_x"));
    mouseSensitivityY = Util.ToFloat(db.SelectSetting("mouse_sensitivity_y"));
    player1Device = (DeviceManager.Devices)Util.ToInt(db.SelectSetting("player1_device"));

    deviceStates = new List<DeviceState>();
    AddDevice(0); // Input player1, as they can be assumed to exist

    Sound.RefreshVolume();
  }

  public static void AddDevice(int joypad){
    if(GetDevice(0) != null){
      return;
    }
    DeviceState ds = new DeviceState(joypad);
    Session.session.deviceStates.Add(ds);
    Session.session.AddChild(ds);
  }

  public static DeviceState GetDevice(int joypad){
    foreach(DeviceState device in Session.session.deviceStates){
      if(device.joypad == joypad){
        return device;
      }
    }
    return null;
  }

  public static void SaveSettings(){
    SettingsDb db = new SettingsDb();
    
    db.StoreSetting("master_volume", "" + Session.session.masterVolume);
    db.StoreSetting("sfx_volume", "" + Session.session.sfxVolume);
    db.StoreSetting("music_volume", "" + Session.session.musicVolume);
    db.StoreSetting("mouse_sensitivity_x", "" + Session.session.mouseSensitivityX);
    db.StoreSetting("mouse_sensitivity_y", "" + Session.session.mouseSensitivityY);
    db.StoreSetting("username", Session.session.userName);
    db.StoreSetting("player1_device", "" + (int)Session.session.player1Device);
    GD.Print("Saving player device as "  + (int)Session.session.player1Device);
    Sound.RefreshVolume();
  }
  
  public void Quit(){
    GetTree().Quit(); 
  }
  
  public static Actor GetPlayer(){
    IGamemode gamemode = Session.session.activeGamemode as IGamemode;
    if(gamemode != null){
      return gamemode.GetPlayer();
    }
    return null;
  }

  public static void InitJukeBox(){
    if(Session.session.jukeBox != null){
      return;
    }

    Session.session.jukeBox = new AudioStreamPlayer();
    Session.session.AddChild(Session.session.jukeBox);
  }
  
  public static System.Random GetRandom(){
    if(Session.session.netSes != null && Session.session.netSes.random != null){
      return Session.session.netSes.random;
    }

    if(Session.session.random != null){
      return Session.session.random;
    }

    Session.session.random = new System.Random();
    return Session.session.random;
  }

  public static bool NetActive(){
    if(session.netSes != null){
      return true;
    }
    
    return false;
  }

  /* Used for syncing items. */
  public static bool IsServer(){
    if(session.netSes != null){
      return session.netSes.isServer;
    }
    
    return false;
  }

  /* Convenience method for creating nodes. */
  public static Node Instance(string path){
    byte[] bytes = Encoding.Default.GetBytes(path);
    path = Encoding.UTF8.GetString(bytes);
    PackedScene packedScene = (PackedScene)GD.Load(path);
    
    if(packedScene == null){
      GD.Print("Path [" + path + "] is invalid." );
      return null;
    }
    
    return packedScene.Instance();
  }
  
  /* Remove game nodes/variables in order to return it to a menu. */
  public static void ClearGame(bool keepNet = false){
    Session ses = Session.session;
    if(ses.activeGamemode != null){
      ses.activeGamemode.QueueFree();
      ses.activeGamemode = null;
    }
    Input.SetMouseMode(Input.MouseMode.Visible);
  }
  
  public static Node GameNode(){
    Session ses = Session.session;
    if(ses.activeGamemode != null){
      return ses.activeGamemode;
    }
    
    return ses;
  }
  
  public static void QuitToMainMenu(){
    Session.ChangeMenu(Menu.Menus.Main);
    Session.ClearGame();
  }

  public static void ChangeMenu(Menu.Menus menu){
    Session ses = Session.session;
    if(ses.activeMenu != null){
      IMenu menuInstance = ses.activeMenu as IMenu;
      
      if(menuInstance != null){
        GD.Print("menuInstance.Clear() " + menu);
        menuInstance.Clear();
      }
      else{
        GD.Print("ChangeMenu.QueueFree ses.activeMenu" + menu);
        ses.activeMenu.QueueFree();
      }
      
      ses.activeMenu = null;
    }
    else{
      GD.Print("ChangeMenu: ses.activeMenu already null when setting " + menu);
    }

    Node createdMenu = Menu.MenuFactory(menu);
    if(ses.activeMenu != null){
      GD.Print("Menu Changed menu in its Init().");
      return;
    }
    else{
      ses.activeMenu = createdMenu;
    }

    if(ses.activeMenu == null){
      GD.Print("Session.ChangeMenu: menu null for " + menu);
    }
  }
  
  private void EnforceSingleton(){
    if(Session.session == null){ Session.session = this; }
    else{ this.QueueFree(); }
  }
  
  public static string GetObjectiveText(){
    Session ses = Session.session;
    IGamemode gamemode = ses.activeGamemode as IGamemode;

    if(gamemode != null){
      return gamemode.GetObjectiveText();
    }
    
    return "No gamemode active";
  }
  
  public void HandleEvent(SessionEvent sessionEvent){
    IGamemode gamemode = Session.session.activeGamemode as IGamemode;

    if(gamemode != null){
      gamemode.HandleEvent(sessionEvent);
    }
  }
  
  public static void Event(SessionEvent sessionEvent){
    Session.session.HandleEvent(sessionEvent);
  }

  public static Vector3 WorldPosition(Item item){
    return item.Translation;
  }
}