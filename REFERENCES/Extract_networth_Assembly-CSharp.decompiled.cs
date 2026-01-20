// SUMMARY OF EXTRACTION
// SOURCE FILE: E:\Brain\the_brain\games\CSharp\MelonLoader\ScheduleI\GameSource\Assembly-CSharp.decompiled.cs
// SEARCH TERM: networth
// TOTAL OCCURRENCES: 78 (VS Code Match)
// TOTAL GROUPS: 18
====================================================================================================


// >>> START OF BLOCK: LINES 68378 TO 68420
// ----------------------------------------
L68378  | 			Awake_UserLogic_ScheduleOne.Property.Business_Assembly-CSharp.dll();
L68379  | 			NetworkInitialize__Late();
L68380  | 		}
L68381  | 
L68382  | 		protected override void Start()
L68383  | 		{
L68384  | 			base.Start();
L68385  | 			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass += new Action(MinPass);
L68386  | 			TimeManager instance = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
L68387  | 			instance.onTimeSkip = (Action<int>)Delegate.Combine(instance.onTimeSkip, new Action<int>(TimeSkipped));
L68388  | 		}
L68389  | 
L68390  | 		protected override void OnDestroy()
L68391  | 		{
L68392  | 			Businesses.Remove(this);
L68393  | 			UnownedBusinesses.Remove(this);
L68394  | 			OwnedBusinesses.Remove(this);
L68395  | 			base.OnDestroy();
L68396  | 		}
L68397  | 
L68398  | 		protected override void GetNetworth(MoneyManager.FloatContainer container)
L68399  | 		{
L68400  | 			base.GetNetworth(container);
L68401  | 			container.ChangeValue(currentLaunderTotal);
L68402  | 		}
L68403  | 
L68404  | 		public override void OnSpawnServer(NetworkConnection connection)
L68405  | 		{
L68406  | 			base.OnSpawnServer(connection);
L68407  | 			if (!connection.IsHost)
L68408  | 			{
L68409  | 				for (int i = 0; i < LaunderingOperations.Count; i++)
L68410  | 				{
L68411  | 					ReceiveLaunderingOperation(connection, LaunderingOperations[i].amount, LaunderingOperations[i].minutesSinceStarted);
L68412  | 				}
L68413  | 			}
L68414  | 		}
L68415  | 
L68416  | 		protected virtual void MinPass()
L68417  | 		{
L68418  | 			MinsPass(1);
L68419  | 		}
L68420  | 

// <<< END OF BLOCK: LINES 68378 TO 68420





// ################################################################################





// >>> START OF BLOCK: LINES 69422 TO 69462
// ----------------------------------------
L69422  | 
L69423  | 		public List<string> LocalExtraFolders { get; set; } = new List<string>();
L69424  | 
L69425  | 		public bool HasChanged { get; set; } = true;
L69426  | 
L69427  | 		public virtual void Awake()
L69428  | 		{
L69429  | 			NetworkInitialize___Early();
L69430  | 			Awake_UserLogic_ScheduleOne.Property.Property_Assembly-CSharp.dll();
L69431  | 			NetworkInitialize__Late();
L69432  | 		}
L69433  | 
L69434  | 		public virtual void InitializeSaveable()
L69435  | 		{
L69436  | 			Singleton<SaveManager>.Instance.RegisterSaveable(this);
L69437  | 		}
L69438  | 
L69439  | 		protected virtual void Start()
L69440  | 		{
L69441  | 			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L69442  | 			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L69443  | 		}
L69444  | 
L69445  | 		protected virtual void FixedUpdate()
L69446  | 		{
L69447  | 			UpdateCulling();
L69448  | 		}
L69449  | 
L69450  | 		public void AddConfigurable(IConfigurable configurable)
L69451  | 		{
L69452  | 			if (!Configurables.Contains(configurable))
L69453  | 			{
L69454  | 				Configurables.Add(configurable);
L69455  | 			}
L69456  | 		}
L69457  | 
L69458  | 		public void RemoveConfigurable(IConfigurable configurable)
L69459  | 		{
L69460  | 			if (Configurables.Contains(configurable))
L69461  | 			{
L69462  | 				Configurables.Remove(configurable);

// <<< END OF BLOCK: LINES 69422 TO 69462





// ################################################################################





// >>> START OF BLOCK: LINES 69487 TO 69534
// ----------------------------------------
L69487  | 		{
L69488  | 			base.OnSpawnServer(connection);
L69489  | 			if (connection.IsHost)
L69490  | 			{
L69491  | 				return;
L69492  | 			}
L69493  | 			for (int i = 0; i < Toggleables.Count; i++)
L69494  | 			{
L69495  | 				if (Toggleables[i].IsActivated)
L69496  | 				{
L69497  | 					SetToggleableState(connection, i, Toggleables[i].IsActivated);
L69498  | 				}
L69499  | 			}
L69500  | 		}
L69501  | 
L69502  | 		protected virtual void OnDestroy()
L69503  | 		{
L69504  | 			if (NetworkSingleton<MoneyManager>.InstanceExists)
L69505  | 			{
L69506  | 				MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L69507  | 				instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L69508  | 			}
L69509  | 			Properties.Remove(this);
L69510  | 			UnownedProperties.Remove(this);
L69511  | 			OwnedProperties.Remove(this);
L69512  | 		}
L69513  | 
L69514  | 		protected virtual void GetNetworth(MoneyManager.FloatContainer container)
L69515  | 		{
L69516  | 			if (IsOwned)
L69517  | 			{
L69518  | 				container.ChangeValue(Price);
L69519  | 			}
L69520  | 		}
L69521  | 
L69522  | 		public override void OnStartServer()
L69523  | 		{
L69524  | 			base.OnStartServer();
L69525  | 			if (OwnedByDefault)
L69526  | 			{
L69527  | 				SetOwned_Server();
L69528  | 			}
L69529  | 			if (base.NetworkObject.GetInitializeOrder() == 0)
L69530  | 			{
L69531  | 				Console.LogError("Property " + PropertyName + " has an initialize order of 0. This will cause issues.");
L69532  | 			}
L69533  | 		}
L69534  | 

// <<< END OF BLOCK: LINES 69487 TO 69534





// ################################################################################





// >>> START OF BLOCK: LINES 78534 TO 78637
// ----------------------------------------
L78534  | 					{
L78535  | 						Console.LogError("Error reading save game data: " + ex3.Message);
L78536  | 					}
L78537  | 					if (!string.IsNullOrEmpty(text2))
L78538  | 					{
L78539  | 						try
L78540  | 						{
L78541  | 							gameData = JsonUtility.FromJson<GameData>(text2);
L78542  | 						}
L78543  | 						catch (Exception ex4)
L78544  | 						{
L78545  | 							gameData = null;
L78546  | 							Console.LogError("Error parsing save game data: " + ex4.Message);
L78547  | 						}
L78548  | 					}
L78549  | 					else
L78550  | 					{
L78551  | 						Console.LogWarning("Game data is empty");
L78552  | 					}
L78553  | 				}
L78554  | 				float networth = 0f;
L78555  | 				string path3 = System.IO.Path.Combine(saveFolderPath, "Money.json");
L78556  | 				MoneyData moneyData = null;
L78557  | 				if (File.Exists(path3))
L78558  | 				{
L78559  | 					string text3 = string.Empty;
L78560  | 					try
L78561  | 					{
L78562  | 						text3 = File.ReadAllText(path3);
L78563  | 					}
L78564  | 					catch (Exception ex5)
L78565  | 					{
L78566  | 						Console.LogError("Error reading save money data: " + ex5.Message);
L78567  | 					}
L78568  | 					if (!string.IsNullOrEmpty(text3))
L78569  | 					{
L78570  | 						try
L78571  | 						{
L78572  | 							moneyData = JsonUtility.FromJson<MoneyData>(text3);
L78573  | 						}
L78574  | 						catch (Exception ex6)
L78575  | 						{
L78576  | 							moneyData = null;
L78577  | 							Console.LogError("Error parsing save money data: " + ex6.Message);
L78578  | 						}
L78579  | 					}
L78580  | 					else
L78581  | 					{
L78582  | 						Console.LogWarning("Money data is empty");
L78583  | 					}
L78584  | 					if (moneyData != null)
L78585  | 					{
L78586  | 						networth = moneyData.Networth;
L78587  | 					}
L78588  | 				}
L78589  | 				if (metaData == null)
L78590  | 				{
L78591  | 					Console.LogWarning("Failed to load metadata. Setting default");
L78592  | 					metaData = new MetaData(new DateTimeData(DateTime.Now), new DateTimeData(DateTime.Now), UnityEngine.Application.version, UnityEngine.Application.version, playTutorial: false);
L78593  | 					try
L78594  | 					{
L78595  | 						File.WriteAllText(path, metaData.GetJson());
L78596  | 					}
L78597  | 					catch (Exception)
L78598  | 					{
L78599  | 					}
L78600  | 				}
L78601  | 				if (gameData == null)
L78602  | 				{
L78603  | 					if (requireGameFile)
L78604  | 					{
L78605  | 						return false;
L78606  | 					}
L78607  | 					Console.LogWarning("Failed to load game data. Setting default");
L78608  | 					gameData = new GameData("Unknown", UnityEngine.Random.Range(0, int.MaxValue), new GameSettings());
L78609  | 					try
L78610  | 					{
L78611  | 						File.WriteAllText(path2, gameData.GetJson());
L78612  | 					}
L78613  | 					catch (Exception)
L78614  | 					{
L78615  | 					}
L78616  | 				}
L78617  | 				saveInfo = new SaveInfo(saveFolderPath, saveSlotIndex, gameData.OrganisationName, metaData.CreationDate.GetDateTime(), metaData.LastPlayedDate.GetDateTime(), networth, metaData.LastSaveVersion, metaData);
L78618  | 				return true;
L78619  | 			}
L78620  | 			return false;
L78621  | 		}
L78622  | 
L78623  | 		public void RefreshSaveInfo()
L78624  | 		{
L78625  | 			for (int i = 0; i < 5; i++)
L78626  | 			{
L78627  | 				SaveGames[i] = null;
L78628  | 				if (TryLoadSaveInfo(System.IO.Path.Combine(Singleton<SaveManager>.Instance.IndividualSavesContainerPath, "SaveGame_" + (i + 1)), i + 1, out var saveInfo))
L78629  | 				{
L78630  | 					SaveGames[i] = saveInfo;
L78631  | 				}
L78632  | 				else
L78633  | 				{
L78634  | 					SaveGames[i] = null;
L78635  | 				}
L78636  | 			}
L78637  | 			LastPlayedGame = null;

// <<< END OF BLOCK: LINES 78534 TO 78637





// ################################################################################





// >>> START OF BLOCK: LINES 78735 TO 78788
// ----------------------------------------
L78735  | 			foreach (KeyValuePair<string, int> item in dictionary)
L78736  | 			{
L78737  | 				TrashIDs[num] = item.Key;
L78738  | 				TrashQuantities[num] = item.Value;
L78739  | 				num++;
L78740  | 			}
L78741  | 		}
L78742  | 	}
L78743  | 	public class SaveInfo
L78744  | 	{
L78745  | 		public string SavePath;
L78746  | 
L78747  | 		public int SaveSlotNumber;
L78748  | 
L78749  | 		public string OrganisationName;
L78750  | 
L78751  | 		public DateTime DateCreated;
L78752  | 
L78753  | 		public DateTime DateLastPlayed;
L78754  | 
L78755  | 		public float Networth;
L78756  | 
L78757  | 		public string SaveVersion;
L78758  | 
L78759  | 		public MetaData MetaData;
L78760  | 
L78761  | 		public SaveInfo(string savePath, int saveSlotNumber, string organisationName, DateTime dateCreated, DateTime dateLastPlayed, float networth, string saveVersion, MetaData metaData)
L78762  | 		{
L78763  | 			SavePath = savePath;
L78764  | 			SaveSlotNumber = saveSlotNumber;
L78765  | 			OrganisationName = organisationName;
L78766  | 			DateCreated = dateCreated;
L78767  | 			DateLastPlayed = dateLastPlayed;
L78768  | 			Networth = networth;
L78769  | 			SaveVersion = saveVersion;
L78770  | 			MetaData = metaData;
L78771  | 		}
L78772  | 	}
L78773  | 	public class SaveManager : PersistentSingleton<SaveManager>
L78774  | 	{
L78775  | 		public const string MAIN_SCENE_NAME = "Main";
L78776  | 
L78777  | 		public const string MENU_SCENE_NAME = "Menu";
L78778  | 
L78779  | 		public const string TUTORIAL_SCENE_NAME = "Tutorial";
L78780  | 
L78781  | 		public const int SAVES_PER_FRAME = 15;
L78782  | 
L78783  | 		public const string SAVE_FILE_EXTENSION = ".json";
L78784  | 
L78785  | 		public const int SAVE_SLOT_COUNT = 5;
L78786  | 
L78787  | 		public const string SAVE_GAME_PREFIX = "SaveGame_";
L78788  | 

// <<< END OF BLOCK: LINES 78735 TO 78788





// ################################################################################





// >>> START OF BLOCK: LINES 84662 TO 84711
// ----------------------------------------
L84662  | 		public string CreationVersion;
L84663  | 
L84664  | 		public string LastSaveVersion;
L84665  | 
L84666  | 		public bool PlayTutorial;
L84667  | 
L84668  | 		public MetaData(DateTimeData creationDate, DateTimeData lastPlayedDate, string creationVersion, string lastSaveVersion, bool playTutorial)
L84669  | 		{
L84670  | 			CreationDate = creationDate;
L84671  | 			LastPlayedDate = lastPlayedDate;
L84672  | 			CreationVersion = creationVersion;
L84673  | 			LastSaveVersion = lastSaveVersion;
L84674  | 			PlayTutorial = playTutorial;
L84675  | 		}
L84676  | 	}
L84677  | 	[Serializable]
L84678  | 	public class MoneyData : SaveData
L84679  | 	{
L84680  | 		public float OnlineBalance;
L84681  | 
L84682  | 		public float Networth;
L84683  | 
L84684  | 		public float LifetimeEarnings;
L84685  | 
L84686  | 		public float WeeklyDepositSum;
L84687  | 
L84688  | 		public MoneyData(float onlineBalance, float netWorth, float lifetimeEarnings, float weeklyDepositSum)
L84689  | 		{
L84690  | 			OnlineBalance = onlineBalance;
L84691  | 			Networth = netWorth;
L84692  | 			LifetimeEarnings = lifetimeEarnings;
L84693  | 			WeeklyDepositSum = weeklyDepositSum;
L84694  | 		}
L84695  | 	}
L84696  | 	[Serializable]
L84697  | 	public class MSGConversationData : SaveData
L84698  | 	{
L84699  | 		public int ConversationIndex;
L84700  | 
L84701  | 		public bool Read;
L84702  | 
L84703  | 		public TextMessageData[] MessageHistory;
L84704  | 
L84705  | 		public TextResponseData[] ActiveResponses;
L84706  | 
L84707  | 		public bool IsHidden;
L84708  | 
L84709  | 		public MSGConversationData(int conversationIndex, bool read, TextMessageData[] messageHistory, TextResponseData[] activeResponses, bool isHidden)
L84710  | 		{
L84711  | 			ConversationIndex = conversationIndex;

// <<< END OF BLOCK: LINES 84662 TO 84711





// ################################################################################





// >>> START OF BLOCK: LINES 85263 TO 85308
// ----------------------------------------
L85263  | 			: base(guid, item, loadOrder, parentSurfaceGUID, pos, rot)
L85264  | 		{
L85265  | 			IsOn = isOn;
L85266  | 		}
L85267  | 	}
L85268  | 	public class TrashContainerData : GridItemData
L85269  | 	{
L85270  | 		public TrashContentData ContentData;
L85271  | 
L85272  | 		public TrashContainerData(Guid guid, ItemInstance item, int loadOrder, Grid grid, Vector2 originCoordinate, int rotation, TrashContentData contentData)
L85273  | 			: base(guid, item, loadOrder, grid, originCoordinate, rotation)
L85274  | 		{
L85275  | 			ContentData = contentData;
L85276  | 		}
L85277  | 	}
L85278  | 	[Serializable]
L85279  | 	public class OrganisationData : SaveData
L85280  | 	{
L85281  | 		public string Name;
L85282  | 
L85283  | 		public float NetWorth;
L85284  | 
L85285  | 		public OrganisationData(string name, float netWorth)
L85286  | 		{
L85287  | 			Name = name;
L85288  | 			NetWorth = netWorth;
L85289  | 		}
L85290  | 	}
L85291  | 	[Serializable]
L85292  | 	public class PlantData : SaveData
L85293  | 	{
L85294  | 		public string SeedID;
L85295  | 
L85296  | 		public float GrowthProgress;
L85297  | 
L85298  | 		public int[] ActiveBuds;
L85299  | 
L85300  | 		public PlantData(string seedID, float growthProgress, int[] activeBuds)
L85301  | 		{
L85302  | 			SeedID = seedID;
L85303  | 			GrowthProgress = growthProgress;
L85304  | 			ActiveBuds = activeBuds;
L85305  | 		}
L85306  | 	}
L85307  | 	[Serializable]
L85308  | 	public class PlayerData : SaveData

// <<< END OF BLOCK: LINES 85263 TO 85308





// ################################################################################





// >>> START OF BLOCK: LINES 97874 TO 97929
// ----------------------------------------
L97874  | 		public static Player GetPlayer(string playerCode)
L97875  | 		{
L97876  | 			return PlayerList.Find((Player x) => x.SyncAccessor_<PlayerCode>k__BackingField == playerCode);
L97877  | 		}
L97878  | 
L97879  | 		public virtual void Awake()
L97880  | 		{
L97881  | 			NetworkInitialize___Early();
L97882  | 			Awake_UserLogic_ScheduleOne.PlayerScripts.Player_Assembly-CSharp.dll();
L97883  | 			NetworkInitialize__Late();
L97884  | 		}
L97885  | 
L97886  | 		public virtual void InitializeSaveable()
L97887  | 		{
L97888  | 			Singleton<SaveManager>.Instance.RegisterSaveable(this);
L97889  | 		}
L97890  | 
L97891  | 		protected virtual void Start()
L97892  | 		{
L97893  | 			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L97894  | 			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L97895  | 			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass += new Action(MinPass);
L97896  | 			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick += new Action(Tick);
L97897  | 		}
L97898  | 
L97899  | 		protected virtual void OnDestroy()
L97900  | 		{
L97901  | 			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
L97902  | 			{
L97903  | 				NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass -= new Action(MinPass);
L97904  | 				NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onTick -= new Action(Tick);
L97905  | 			}
L97906  | 			if (NetworkSingleton<MoneyManager>.InstanceExists)
L97907  | 			{
L97908  | 				MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L97909  | 				instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L97910  | 			}
L97911  | 		}
L97912  | 
L97913  | 		public override void OnStartClient()
L97914  | 		{
L97915  | 			base.OnStartClient();
L97916  | 			Connection = base.Owner;
L97917  | 			if (base.IsOwner)
L97918  | 			{
L97919  | 				if (UnityEngine.Application.isEditor)
L97920  | 				{
L97921  | 					LoadDebugAvatarSettings();
L97922  | 				}
L97923  | 				LocalGameObject.gameObject.SetActive(value: true);
L97924  | 				Local = this;
L97925  | 				if (onLocalPlayerSpawned != null)
L97926  | 				{
L97927  | 					onLocalPlayerSpawned();
L97928  | 				}
L97929  | 				LayerUtility.SetLayerRecursively(Avatar.gameObject, LayerMask.NameToLayer("Invisible"));

// <<< END OF BLOCK: LINES 97874 TO 97929





// ################################################################################





// >>> START OF BLOCK: LINES 99190 TO 99230
// ----------------------------------------
L99190  | 		{
L99191  | 			RpcWriter___Server_SetEquippedSlotIndex_3316948804(index);
L99192  | 			RpcLogic___SetEquippedSlotIndex_3316948804(index);
L99193  | 		}
L99194  | 
L99195  | 		public ItemInstance GetEquippedItem()
L99196  | 		{
L99197  | 			if (EquippedItemSlotIndex == -1)
L99198  | 			{
L99199  | 				return null;
L99200  | 			}
L99201  | 			return Inventory[EquippedItemSlotIndex].ItemInstance;
L99202  | 		}
L99203  | 
L99204  | 		[ObserversRpc]
L99205  | 		public void RemoveEquippedItemFromInventory(string id, int amount)
L99206  | 		{
L99207  | 			RpcWriter___Observers_RemoveEquippedItemFromInventory_3643459082(id, amount);
L99208  | 		}
L99209  | 
L99210  | 		private void GetNetworth(MoneyManager.FloatContainer container)
L99211  | 		{
L99212  | 			for (int i = 0; i < Inventory.Length; i++)
L99213  | 			{
L99214  | 				if (Inventory[i].ItemInstance != null)
L99215  | 				{
L99216  | 					container.ChangeValue(Inventory[i].ItemInstance.GetMonetaryValue());
L99217  | 				}
L99218  | 			}
L99219  | 		}
L99220  | 
L99221  | 		[ServerRpc(RunLocally = true)]
L99222  | 		public void SendAppearance(BasicAvatarSettings settings)
L99223  | 		{
L99224  | 			RpcWriter___Server_SendAppearance_3281254764(settings);
L99225  | 			RpcLogic___SendAppearance_3281254764(settings);
L99226  | 		}
L99227  | 
L99228  | 		[ObserversRpc(RunLocally = true)]
L99229  | 		public void SetAppearance(BasicAvatarSettings settings, bool refreshClothing)
L99230  | 		{

// <<< END OF BLOCK: LINES 99190 TO 99230





// ################################################################################





// >>> START OF BLOCK: LINES 142197 TO 142237
// ----------------------------------------
L142197 | 			NetworkInitialize___Early();
L142198 | 			base.Awake();
L142199 | 			NetworkInitialize__Late();
L142200 | 		}
L142201 | 	}
L142202 | 	public class Ray : NPC
L142203 | 	{
L142204 | 		public DialogueContainer GreetingDialogue;
L142205 | 
L142206 | 		public string GreetedVariable = "RayGreeted";
L142207 | 
L142208 | 		public string IntroductionMessage;
L142209 | 
L142210 | 		public string IntroSentVariable = "RayIntroSent";
L142211 | 
L142212 | 		[Header("Intro message conditions")]
L142213 | 		public FullRank IntroRank;
L142214 | 
L142215 | 		public int IntroDaysPlayed = 21;
L142216 | 
L142217 | 		public float IntroNetworth = 15000f;
L142218 | 
L142219 | 		private bool NetworkInitialize___EarlyScheduleOne.NPCs.CharacterClasses.RayAssembly-CSharp.dll_Excuted;
L142220 | 
L142221 | 		private bool NetworkInitialize__LateScheduleOne.NPCs.CharacterClasses.RayAssembly-CSharp.dll_Excuted;
L142222 | 
L142223 | 		protected override void Start()
L142224 | 		{
L142225 | 			base.Start();
L142226 | 			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(Loaded);
L142227 | 			Manor manor = ScheduleOne.Property.Property.Properties.Find((ScheduleOne.Property.Property x) => x is Manor) as Manor;
L142228 | 			if (manor != null)
L142229 | 			{
L142230 | 				manor.onRebuildComplete = (Action)Delegate.Combine(manor.onRebuildComplete, new Action(NotifyPlayerOfManorRebuild));
L142231 | 			}
L142232 | 		}
L142233 | 
L142234 | 		private void Loaded()
L142235 | 		{
L142236 | 			Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(Loaded);
L142237 | 			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>(GreetedVariable))

// <<< END OF BLOCK: LINES 142197 TO 142237





// ################################################################################





// >>> START OF BLOCK: LINES 182115 TO 182198
// ----------------------------------------
L182115 | 
L182116 | 		public void SetGUID(Guid guid)
L182117 | 		{
L182118 | 			GUID = guid;
L182119 | 			GUIDManager.RegisterObject(this);
L182120 | 		}
L182121 | 
L182122 | 		protected virtual void Start()
L182123 | 		{
L182124 | 			intObj.onHovered.AddListener(Hovered);
L182125 | 			intObj.onInteractStart.AddListener(Interacted);
L182126 | 			if (centerOfMass != null)
L182127 | 			{
L182128 | 				Rb.centerOfMass = base.transform.InverseTransformPoint(centerOfMass.transform.position);
L182129 | 			}
L182130 | 			if (GUID == Guid.Empty)
L182131 | 			{
L182132 | 				GUID = GUIDManager.GenerateUniqueGUID();
L182133 | 			}
L182134 | 			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L182135 | 			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L182136 | 			GameInput.RegisterExitListener(Exit);
L182137 | 			if (UseHumanoidCollider)
L182138 | 			{
L182139 | 				HumanoidColliderContainer.Vehicle = this;
L182140 | 				HumanoidColliderContainer.transform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
L182141 | 			}
L182142 | 			else
L182143 | 			{
L182144 | 				HumanoidColliderContainer.gameObject.SetActive(value: false);
L182145 | 			}
L182146 | 			if (!NetworkSingleton<VehicleManager>.Instance.AllVehicles.Contains(this))
L182147 | 			{
L182148 | 				NetworkSingleton<VehicleManager>.Instance.AllVehicles.Add(this);
L182149 | 			}
L182150 | 		}
L182151 | 
L182152 | 		private void Exit(ExitAction action)
L182153 | 		{
L182154 | 			if (!action.Used && action.exitType == ExitType.Escape && LocalPlayerIsInVehicle)
L182155 | 			{
L182156 | 				action.Used = true;
L182157 | 				ExitVehicle();
L182158 | 			}
L182159 | 		}
L182160 | 
L182161 | 		protected virtual void OnDestroy()
L182162 | 		{
L182163 | 			if (NetworkSingleton<MoneyManager>.InstanceExists)
L182164 | 			{
L182165 | 				MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L182166 | 				instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L182167 | 			}
L182168 | 			if (HumanoidColliderContainer != null)
L182169 | 			{
L182170 | 				UnityEngine.Object.Destroy(HumanoidColliderContainer.gameObject);
L182171 | 			}
L182172 | 			if (NetworkSingleton<VehicleManager>.InstanceExists)
L182173 | 			{
L182174 | 				NetworkSingleton<VehicleManager>.Instance.AllVehicles.Remove(this);
L182175 | 			}
L182176 | 		}
L182177 | 
L182178 | 		private void GetNetworth(MoneyManager.FloatContainer container)
L182179 | 		{
L182180 | 			if (IsPlayerOwned)
L182181 | 			{
L182182 | 				container.ChangeValue(GetVehicleValue());
L182183 | 			}
L182184 | 		}
L182185 | 
L182186 | 		protected virtual void Update()
L182187 | 		{
L182188 | 			if (PlayerSingleton<PlayerCamera>.InstanceExists && base.IsSpawned)
L182189 | 			{
L182190 | 				bool flag = LocalPlayerIsDriver || base.IsOwner || (base.OwnerId == -1 && InstanceFinder.IsHost);
L182191 | 				Rb.interpolation = (flag ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
L182192 | 				if (LocalPlayerIsInVehicle && GameInput.GetButtonDown(GameInput.ButtonCode.Interact) && !GameInput.IsTyping)
L182193 | 				{
L182194 | 					ExitVehicle();
L182195 | 				}
L182196 | 				if (overrideControls)
L182197 | 				{
L182198 | 					currentThrottle = throttleOverride;

// <<< END OF BLOCK: LINES 182115 TO 182198





// ################################################################################





// >>> START OF BLOCK: LINES 198037 TO 198099
// ----------------------------------------
L198037 | 		private bool NetworkInitialize__LateScheduleOne.Storage.StorageEntityAssembly-CSharp.dll_Excuted;
L198038 | 
L198039 | 		public bool IsOpened => CurrentPlayerAccessor != null;
L198040 | 
L198041 | 		public Player CurrentPlayerAccessor { get; protected set; }
L198042 | 
L198043 | 		public int ItemCount => ((IItemSlotOwner)this).GetQuantitySum();
L198044 | 
L198045 | 		public List<ItemSlot> ItemSlots { get; set; } = new List<ItemSlot>();
L198046 | 
L198047 | 		public virtual void Awake()
L198048 | 		{
L198049 | 			NetworkInitialize___Early();
L198050 | 			Awake_UserLogic_ScheduleOne.Storage.StorageEntity_Assembly-CSharp.dll();
L198051 | 			NetworkInitialize__Late();
L198052 | 		}
L198053 | 
L198054 | 		protected virtual void Start()
L198055 | 		{
L198056 | 			MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L198057 | 			instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Combine(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L198058 | 			if (EmptyOnSleep)
L198059 | 			{
L198060 | 				TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
L198061 | 				instance2.onSleepStart = (Action)Delegate.Combine(instance2.onSleepStart, new Action(ClearContents));
L198062 | 			}
L198063 | 		}
L198064 | 
L198065 | 		protected virtual void OnDestroy()
L198066 | 		{
L198067 | 			if (NetworkSingleton<MoneyManager>.InstanceExists)
L198068 | 			{
L198069 | 				MoneyManager instance = NetworkSingleton<MoneyManager>.Instance;
L198070 | 				instance.onNetworthCalculation = (Action<MoneyManager.FloatContainer>)Delegate.Remove(instance.onNetworthCalculation, new Action<MoneyManager.FloatContainer>(GetNetworth));
L198071 | 			}
L198072 | 			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
L198073 | 			{
L198074 | 				TimeManager instance2 = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
L198075 | 				instance2.onSleepStart = (Action)Delegate.Remove(instance2.onSleepStart, new Action(ClearContents));
L198076 | 			}
L198077 | 		}
L198078 | 
L198079 | 		private void GetNetworth(MoneyManager.FloatContainer container)
L198080 | 		{
L198081 | 			for (int i = 0; i < ItemSlots.Count; i++)
L198082 | 			{
L198083 | 				if (ItemSlots[i].ItemInstance != null)
L198084 | 				{
L198085 | 					container.ChangeValue(ItemSlots[i].ItemInstance.GetMonetaryValue());
L198086 | 				}
L198087 | 			}
L198088 | 		}
L198089 | 
L198090 | 		public override void OnSpawnServer(NetworkConnection connection)
L198091 | 		{
L198092 | 			base.OnSpawnServer(connection);
L198093 | 			if (!connection.IsHost)
L198094 | 			{
L198095 | 				ReplicationQueue.Enqueue(GetType().Name, connection, ReplicateInventory, 10 + ((IItemSlotOwner)this).GetNonEmptySlotCount() * 80);
L198096 | 			}
L198097 | 			void ReplicateInventory(NetworkConnection conn)
L198098 | 			{
L198099 | 				((IItemSlotOwner)this).SendItemSlotDataToClient(conn);

// <<< END OF BLOCK: LINES 198037 TO 198099





// ################################################################################





// >>> START OF BLOCK: LINES 231142 TO 231209
// ----------------------------------------
L231142 | 				while (Group.alpha > 0f)
L231143 | 				{
L231144 | 					Group.alpha -= Time.deltaTime * 2f;
L231145 | 					yield return null;
L231146 | 				}
L231147 | 				base.gameObject.SetActive(value: false);
L231148 | 			}
L231149 | 		}
L231150 | 	}
L231151 | 	public class ImportScreen : MainMenuScreen
L231152 | 	{
L231153 | 		[Header("References")]
L231154 | 		public GameObject MainContainer;
L231155 | 
L231156 | 		public GameObject FailContainer;
L231157 | 
L231158 | 		public UnityEngine.UI.Button ConfirmButton;
L231159 | 
L231160 | 		public TextMeshProUGUI OrganisationNameLabel;
L231161 | 
L231162 | 		public TextMeshProUGUI NetworthLabel;
L231163 | 
L231164 | 		public TextMeshProUGUI VersionLabel;
L231165 | 
L231166 | 		public TextMeshProUGUI WarningLabel;
L231167 | 
L231168 | 		private int slotToOverwrite;
L231169 | 
L231170 | 		private SaveInfo saveInfo;
L231171 | 
L231172 | 		public void Initialize(int _slotToOverwrite, MainMenuScreen previousScreen)
L231173 | 		{
L231174 | 			slotToOverwrite = _slotToOverwrite;
L231175 | 			PreviousScreen = previousScreen;
L231176 | 			bool flag = false;
L231177 | 			string tempImportPath = SaveImportButton.TempImportPath;
L231178 | 			string[] directories = Directory.GetDirectories(tempImportPath);
L231179 | 			if (directories.Length != 0)
L231180 | 			{
L231181 | 				string fileName = System.IO.Path.GetFileName(directories[0]);
L231182 | 				string text = System.IO.Path.Combine(tempImportPath, fileName);
L231183 | 				if (LoadManager.TryLoadSaveInfo(text, -1, out var saveInfo, requireGameFile: true))
L231184 | 				{
L231185 | 					Console.Log("Loaded save info from: " + text);
L231186 | 					this.saveInfo = saveInfo;
L231187 | 					flag = true;
L231188 | 					OrganisationNameLabel.text = saveInfo.OrganisationName;
L231189 | 					NetworthLabel.text = MoneyManager.FormatAmount(saveInfo.Networth);
L231190 | 					VersionLabel.text = "v" + saveInfo.SaveVersion;
L231191 | 					if (LoadManager.SaveGames[slotToOverwrite] != null)
L231192 | 					{
L231193 | 						WarningLabel.text = "Warning: This will overwrite the current save in slot " + (slotToOverwrite + 1) + " (" + LoadManager.SaveGames[slotToOverwrite].OrganisationName + ").";
L231194 | 						WarningLabel.enabled = true;
L231195 | 					}
L231196 | 					else
L231197 | 					{
L231198 | 						WarningLabel.enabled = false;
L231199 | 					}
L231200 | 				}
L231201 | 			}
L231202 | 			ConfirmButton.interactable = flag;
L231203 | 			MainContainer.SetActive(flag);
L231204 | 			FailContainer.SetActive(!flag);
L231205 | 		}
L231206 | 
L231207 | 		public void Cancel()
L231208 | 		{
L231209 | 			Close(openPrevious: true);

// <<< END OF BLOCK: LINES 231142 TO 231209





// ################################################################################





// >>> START OF BLOCK: LINES 231310 TO 231350
// ----------------------------------------
L231310 | 		public void Awake()
L231311 | 		{
L231312 | 			Singleton<LoadManager>.Instance.onSaveInfoLoaded.AddListener(LoadStuff);
L231313 | 		}
L231314 | 
L231315 | 		private void LoadStuff()
L231316 | 		{
L231317 | 			bool flag = false;
L231318 | 			if (LoadManager.LastPlayedGame != null)
L231319 | 			{
L231320 | 				string text = System.IO.Path.Combine(System.IO.Path.Combine(LoadManager.LastPlayedGame.SavePath, "Players", "Player_0"), "Appearance.json");
L231321 | 				if (File.Exists(text))
L231322 | 				{
L231323 | 					string json = File.ReadAllText(text);
L231324 | 					BasicAvatarSettings basicAvatarSettings = ScriptableObject.CreateInstance<BasicAvatarSettings>();
L231325 | 					JsonUtility.FromJsonOverwrite(json, basicAvatarSettings);
L231326 | 					Avatar.LoadAvatarSettings(basicAvatarSettings.GetAvatarSettings());
L231327 | 					flag = true;
L231328 | 					Console.Log("Loaded player appearance from " + text);
L231329 | 				}
L231330 | 				float num = LoadManager.LastPlayedGame.Networth;
L231331 | 				for (int i = 0; i < CashPiles.Length; i++)
L231332 | 				{
L231333 | 					float displayedAmount = Mathf.Clamp(num, 0f, 100000f);
L231334 | 					CashPiles[i].SetDisplayedAmount(displayedAmount);
L231335 | 					num -= 100000f;
L231336 | 					if (num <= 0f)
L231337 | 					{
L231338 | 						break;
L231339 | 					}
L231340 | 				}
L231341 | 			}
L231342 | 			if (!flag)
L231343 | 			{
L231344 | 				Avatar.gameObject.SetActive(value: false);
L231345 | 			}
L231346 | 		}
L231347 | 	}
L231348 | 	public class MainMenuScreen : MonoBehaviour
L231349 | 	{
L231350 | 		public const float LERP_TIME = 0.075f;

// <<< END OF BLOCK: LINES 231310 TO 231350





// ################################################################################





// >>> START OF BLOCK: LINES 231500 TO 231559
// ----------------------------------------
L231500 | 		}
L231501 | 
L231502 | 		public void Refresh()
L231503 | 		{
L231504 | 			for (int i = 0; i < LoadManager.SaveGames.Length; i++)
L231505 | 			{
L231506 | 				SetDisplayedSave(i, LoadManager.SaveGames[i]);
L231507 | 			}
L231508 | 		}
L231509 | 
L231510 | 		public void SetDisplayedSave(int index, SaveInfo info)
L231511 | 		{
L231512 | 			Transform transform = Slots[index].Find("Container");
L231513 | 			if (info == null)
L231514 | 			{
L231515 | 				transform.Find("Info").gameObject.SetActive(value: false);
L231516 | 				return;
L231517 | 			}
L231518 | 			transform.Find("Info/Organisation").GetComponent<TextMeshProUGUI>().text = info.OrganisationName;
L231519 | 			transform.Find("Info/Version").GetComponent<TextMeshProUGUI>().text = "v" + info.SaveVersion;
L231520 | 			float networth = info.Networth;
L231521 | 			string empty = string.Empty;
L231522 | 			Color color = new Color32(75, byte.MaxValue, 10, byte.MaxValue);
L231523 | 			if (networth > 1000000f)
L231524 | 			{
L231525 | 				networth /= 1000000f;
L231526 | 				empty = "$" + RoundToDecimalPlaces(networth, 1) + "M";
L231527 | 				color = new Color32(byte.MaxValue, 225, 10, byte.MaxValue);
L231528 | 			}
L231529 | 			else if (networth > 1000f)
L231530 | 			{
L231531 | 				networth /= 1000f;
L231532 | 				empty = "$" + RoundToDecimalPlaces(networth, 1) + "K";
L231533 | 			}
L231534 | 			else
L231535 | 			{
L231536 | 				empty = MoneyManager.FormatAmount(networth);
L231537 | 			}
L231538 | 			transform.Find("Info/NetWorth/Text").GetComponent<TextMeshProUGUI>().text = empty;
L231539 | 			transform.Find("Info/NetWorth/Text").GetComponent<TextMeshProUGUI>().color = color;
L231540 | 			int hours = Mathf.RoundToInt((float)(DateTime.Now - info.DateCreated).TotalHours);
L231541 | 			transform.Find("Info/Created/Text").GetComponent<TextMeshProUGUI>().text = GetTimeLabel(hours);
L231542 | 			int hours2 = Mathf.RoundToInt((float)(DateTime.Now - info.DateLastPlayed).TotalHours);
L231543 | 			transform.Find("Info/LastPlayed/Text").GetComponent<TextMeshProUGUI>().text = GetTimeLabel(hours2);
L231544 | 			transform.Find("Info").gameObject.SetActive(value: true);
L231545 | 		}
L231546 | 
L231547 | 		private float RoundToDecimalPlaces(float value, int decimalPlaces)
L231548 | 		{
L231549 | 			return ToSingle(System.Math.Floor((double)value * System.Math.Pow(10.0, decimalPlaces)) / System.Math.Pow(10.0, decimalPlaces));
L231550 | 		}
L231551 | 
L231552 | 		public static float ToSingle(double value)
L231553 | 		{
L231554 | 			return (float)value;
L231555 | 		}
L231556 | 
L231557 | 		private string GetTimeLabel(int hours)
L231558 | 		{
L231559 | 			int num = hours / 24;

// <<< END OF BLOCK: LINES 231500 TO 231559





// ################################################################################





// >>> START OF BLOCK: LINES 247685 TO 247739
// ----------------------------------------
L247685 | 		public List<Transaction> ledger = new List<Transaction>();
L247686 | 
L247687 | 		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
L247688 | 		public float onlineBalance;
L247689 | 
L247690 | 		[SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)]
L247691 | 		public float lifetimeEarnings;
L247692 | 
L247693 | 		[SerializeField]
L247694 | 		protected AudioSourceController CashSound;
L247695 | 
L247696 | 		[Header("Prefabs")]
L247697 | 		[SerializeField]
L247698 | 		protected GameObject moneyChangePrefab;
L247699 | 
L247700 | 		[SerializeField]
L247701 | 		protected GameObject cashChangePrefab;
L247702 | 
L247703 | 		public Sprite LaunderingNotificationIcon;
L247704 | 
L247705 | 		public Action<FloatContainer> onNetworthCalculation;
L247706 | 
L247707 | 		private MoneyLoader loader = new MoneyLoader();
L247708 | 
L247709 | 		public SyncVar<float> syncVar___onlineBalance;
L247710 | 
L247711 | 		public SyncVar<float> syncVar___lifetimeEarnings;
L247712 | 
L247713 | 		private bool NetworkInitialize___EarlyScheduleOne.Money.MoneyManagerAssembly-CSharp.dll_Excuted;
L247714 | 
L247715 | 		private bool NetworkInitialize__LateScheduleOne.Money.MoneyManagerAssembly-CSharp.dll_Excuted;
L247716 | 
L247717 | 		public float LifetimeEarnings => SyncAccessor_lifetimeEarnings;
L247718 | 
L247719 | 		public float LastCalculatedNetworth { get; protected set; }
L247720 | 
L247721 | 		public float cashBalance => cashInstance.Balance;
L247722 | 
L247723 | 		protected CashInstance cashInstance => PlayerSingleton<PlayerInventory>.Instance.cashInstance;
L247724 | 
L247725 | 		public string SaveFolderName => "Money";
L247726 | 
L247727 | 		public string SaveFileName => "Money";
L247728 | 
L247729 | 		public Loader Loader => loader;
L247730 | 
L247731 | 		public bool ShouldSaveUnderFolder => false;
L247732 | 
L247733 | 		public List<string> LocalExtraFiles { get; set; } = new List<string>();
L247734 | 
L247735 | 		public List<string> LocalExtraFolders { get; set; } = new List<string>();
L247736 | 
L247737 | 		public bool HasChanged { get; set; }
L247738 | 
L247739 | 		public int LoadOrder { get; }

// <<< END OF BLOCK: LINES 247685 TO 247739





// ################################################################################





// >>> START OF BLOCK: LINES 247792 TO 247868
// ----------------------------------------
L247792 | 		}
L247793 | 
L247794 | 		public override void Awake()
L247795 | 		{
L247796 | 			NetworkInitialize___Early();
L247797 | 			Awake_UserLogic_ScheduleOne.Money.MoneyManager_Assembly-CSharp.dll();
L247798 | 			NetworkInitialize__Late();
L247799 | 		}
L247800 | 
L247801 | 		public virtual void InitializeSaveable()
L247802 | 		{
L247803 | 			Singleton<SaveManager>.Instance.RegisterSaveable(this);
L247804 | 		}
L247805 | 
L247806 | 		protected override void Start()
L247807 | 		{
L247808 | 			base.Start();
L247809 | 			Singleton<LoadManager>.Instance.onLoadComplete.AddListener(Loaded);
L247810 | 			NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass += new Action(MinPass);
L247811 | 			TimeManager timeManager = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
L247812 | 			timeManager.onDayPass = (Action)Delegate.Combine(timeManager.onDayPass, new Action(CheckNetworthAchievements));
L247813 | 			Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
L247814 | 		}
L247815 | 
L247816 | 		public override void OnStartServer()
L247817 | 		{
L247818 | 			base.OnStartServer();
L247819 | 			if (NetworkSingleton<VariableDatabase>.InstanceExists)
L247820 | 			{
L247821 | 				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LifetimeEarnings", lifetimeEarnings.ToString());
L247822 | 			}
L247823 | 		}
L247824 | 
L247825 | 		public override void OnStartClient()
L247826 | 		{
L247827 | 			base.OnStartClient();
L247828 | 			Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
L247829 | 		}
L247830 | 
L247831 | 		protected override void OnDestroy()
L247832 | 		{
L247833 | 			base.OnDestroy();
L247834 | 			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.InstanceExists)
L247835 | 			{
L247836 | 				NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.onMinutePass -= new Action(MinPass);
L247837 | 				TimeManager timeManager = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
L247838 | 				timeManager.onDayPass = (Action)Delegate.Remove(timeManager.onDayPass, new Action(CheckNetworthAchievements));
L247839 | 			}
L247840 | 			if (Singleton<LoadManager>.InstanceExists)
L247841 | 			{
L247842 | 				Singleton<LoadManager>.Instance.onLoadComplete.RemoveListener(Loaded);
L247843 | 			}
L247844 | 		}
L247845 | 
L247846 | 		private void Loaded()
L247847 | 		{
L247848 | 			GetNetWorth();
L247849 | 			Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
L247850 | 		}
L247851 | 
L247852 | 		private void Update()
L247853 | 		{
L247854 | 			HasChanged = true;
L247855 | 		}
L247856 | 
L247857 | 		private void MinPass()
L247858 | 		{
L247859 | 			if (NetworkSingleton<VariableDatabase>.InstanceExists)
L247860 | 			{
L247861 | 				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Online_Balance", onlineBalance.ToString(), network: false);
L247862 | 				if (PlayerSingleton<PlayerInventory>.InstanceExists)
L247863 | 				{
L247864 | 					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Cash_Balance", cashBalance.ToString(), network: false);
L247865 | 					NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Total_Money", (SyncAccessor_onlineBalance + cashBalance).ToString(), network: false);
L247866 | 				}
L247867 | 			}
L247868 | 		}

// <<< END OF BLOCK: LINES 247792 TO 247868





// ################################################################################





// >>> START OF BLOCK: LINES 247963 TO 248041
// ----------------------------------------
L247963 | 		{
L247964 | 			string text = string.Empty;
L247965 | 			if (includeColor)
L247966 | 			{
L247967 | 				text += "<color=#54E717>";
L247968 | 			}
L247969 | 			if (amount < 0f)
L247970 | 			{
L247971 | 				text = "-";
L247972 | 			}
L247973 | 			text = ((!showDecimals) ? (text + string.Format(new CultureInfo("en-US"), "{0:C0}", Mathf.RoundToInt(Mathf.Abs(amount)))) : (text + string.Format(new CultureInfo("en-US"), "{0:C}", Mathf.Abs(amount))));
L247974 | 			if (includeColor)
L247975 | 			{
L247976 | 				text += "</color>";
L247977 | 			}
L247978 | 			return text;
L247979 | 		}
L247980 | 
L247981 | 		public virtual string GetSaveString()
L247982 | 		{
L247983 | 			return new MoneyData(SyncAccessor_onlineBalance, GetNetWorth(), SyncAccessor_lifetimeEarnings, ATM.WeeklyDepositSum).GetJson();
L247984 | 		}
L247985 | 
L247986 | 		public void Load(MoneyData data)
L247987 | 		{
L247988 | 			this.sync___set_value_onlineBalance(Mathf.Clamp(data.OnlineBalance, 0f, float.MaxValue), asServer: true);
L247989 | 			this.sync___set_value_lifetimeEarnings(Mathf.Clamp(data.LifetimeEarnings, 0f, float.MaxValue), asServer: true);
L247990 | 			Singleton<HUD>.Instance.OnlineBalanceDisplay.SetBalance(SyncAccessor_onlineBalance);
L247991 | 			ATM.WeeklyDepositSum = data.WeeklyDepositSum;
L247992 | 		}
L247993 | 
L247994 | 		public void CheckNetworthAchievements()
L247995 | 		{
L247996 | 			float netWorth = GetNetWorth();
L247997 | 			if (netWorth >= 100000f)
L247998 | 			{
L247999 | 				Singleton<AchievementManager>.Instance.UnlockAchievement(AchievementManager.EAchievement.BUSINESSMAN);
L248000 | 			}
L248001 | 			if (netWorth >= 1000000f)
L248002 | 			{
L248003 | 				Singleton<AchievementManager>.Instance.UnlockAchievement(AchievementManager.EAchievement.BIGWIG);
L248004 | 			}
L248005 | 			if (netWorth >= 10000000f)
L248006 | 			{
L248007 | 				Singleton<AchievementManager>.Instance.UnlockAchievement(AchievementManager.EAchievement.MAGNATE);
L248008 | 			}
L248009 | 		}
L248010 | 
L248011 | 		public float GetNetWorth()
L248012 | 		{
L248013 | 			float num = 0f;
L248014 | 			num += SyncAccessor_onlineBalance;
L248015 | 			if (onNetworthCalculation != null)
L248016 | 			{
L248017 | 				FloatContainer floatContainer = new FloatContainer();
L248018 | 				onNetworthCalculation(floatContainer);
L248019 | 				num += floatContainer.value;
L248020 | 			}
L248021 | 			LastCalculatedNetworth = num;
L248022 | 			return num;
L248023 | 		}
L248024 | 
L248025 | 		public override void NetworkInitialize___Early()
L248026 | 		{
L248027 | 			if (!NetworkInitialize___EarlyScheduleOne.Money.MoneyManagerAssembly-CSharp.dll_Excuted)
L248028 | 			{
L248029 | 				NetworkInitialize___EarlyScheduleOne.Money.MoneyManagerAssembly-CSharp.dll_Excuted = true;
L248030 | 				base.NetworkInitialize___Early();
L248031 | 				syncVar___lifetimeEarnings = new SyncVar<float>(this, 1u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, lifetimeEarnings);
L248032 | 				syncVar___onlineBalance = new SyncVar<float>(this, 0u, WritePermission.ClientUnsynchronized, ReadPermission.Observers, -1f, Channel.Reliable, onlineBalance);
L248033 | 				RegisterServerRpc(0u, RpcReader___Server_CreateOnlineTransaction_1419830531);
L248034 | 				RegisterObserversRpc(1u, RpcReader___Observers_ReceiveOnlineTransaction_1419830531);
L248035 | 				RegisterServerRpc(2u, RpcReader___Server_ChangeLifetimeEarnings_431000436);
L248036 | 				RegisterSyncVarRead(ReadSyncVar___ScheduleOne.Money.MoneyManager);
L248037 | 			}
L248038 | 		}
L248039 | 
L248040 | 		public override void NetworkInitialize__Late()
L248041 | 		{

// <<< END OF BLOCK: LINES 247963 TO 248041





// ################################################################################





