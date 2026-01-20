// SUMMARY OF EXTRACTION
// SOURCE FILE: E:\Brain\the_brain\games\CSharp\MelonLoader\ScheduleI\GameSource\Assembly-CSharp.decompiled.cs
// SEARCH TERM: MoneyManager
// TOTAL OCCURRENCES: 278 (VS Code Match)
// TOTAL GROUPS: 92
====================================================================================================


// >>> START OF BLOCK: LINES 54991 TO 55067
// ----------------------------------------
L54991  | 			}
L54992  | 		}
L54993  | 
L54994  | 		public class ChangeCashCommand : ConsoleCommand
L54995  | 		{
L54996  | 			public override string CommandWord => "changecash";
L54997  | 
L54998  | 			public override string CommandDescription => "Changes the player's cash balance by the specified amount";
L54999  | 
L55000  | 			public override string ExampleUsage => "changecash 5000";
L55001  | 
L55002  | 			public override void Execute(List<string> args)
L55003  | 			{
L55004  | 				float result = 0f;
L55005  | 				if (args.Count == 0 || !float.TryParse(args[0], out result))
L55006  | 				{
L55007  | 					LogWarning("Unrecognized command format. Correct format example(s): 'changecash 5000', 'changecash -5000'");
L55008  | 				}
L55009  | 				else if (result > 0f)
L55010  | 				{
L55011  | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result);
L55012  | 					Log("Gave player " + MoneyManager.FormatAmount(result) + " cash");
L55013  | 				}
L55014  | 				else if (result < 0f)
L55015  | 				{
L55016  | 					result = Mathf.Clamp(result, 0f - NetworkSingleton<MoneyManager>.Instance.cashBalance, 0f);
L55017  | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result);
L55018  | 					Log("Removed " + MoneyManager.FormatAmount(result) + " cash from player");
L55019  | 				}
L55020  | 			}
L55021  | 		}
L55022  | 
L55023  | 		public class ChangeOnlineBalanceCommand : ConsoleCommand
L55024  | 		{
L55025  | 			public override string CommandWord => "changebalance";
L55026  | 
L55027  | 			public override string CommandDescription => "Changes the player's online balance by the specified amount";
L55028  | 
L55029  | 			public override string ExampleUsage => "changebalance 5000";
L55030  | 
L55031  | 			public override void Execute(List<string> args)
L55032  | 			{
L55033  | 				float result = 0f;
L55034  | 				if (args.Count == 0 || !float.TryParse(args[0], out result))
L55035  | 				{
L55036  | 					LogWarning("Unrecognized command format. Correct format example(s): 'changebalance 5000', 'changebalance -5000'");
L55037  | 				}
L55038  | 				else if (result > 0f)
L55039  | 				{
L55040  | 					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Added online balance", result, 1f, "Added by developer console");
L55041  | 					Log("Increased online balance by " + MoneyManager.FormatAmount(result));
L55042  | 				}
L55043  | 				else if (result < 0f)
L55044  | 				{
L55045  | 					result = Mathf.Clamp(result, 0f - NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance, 0f);
L55046  | 					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Removed online balance", result, 1f, "Removed by developer console");
L55047  | 					Log("Decreased online balance by " + MoneyManager.FormatAmount(result));
L55048  | 				}
L55049  | 			}
L55050  | 		}
L55051  | 
L55052  | 		public class SetMoveSpeedCommand : ConsoleCommand
L55053  | 		{
L55054  | 			public override string CommandWord => "setmovespeed";
L55055  | 
L55056  | 			public override string CommandDescription => "Sets the player's move speed multiplier";
L55057  | 
L55058  | 			public override string ExampleUsage => "setmovespeed 1";
L55059  | 
L55060  | 			public override void Execute(List<string> args)
L55061  | 			{
L55062  | 				float result = 0f;
L55063  | 				if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
L55064  | 				{
L55065  | 					LogWarning("Unrecognized command format. Correct format example(s): 'setmovespeed 1'");
L55066  | 					return;
L55067  | 				}

// <<< END OF BLOCK: LINES 54991 TO 55067





// ################################################################################





// >>> START OF BLOCK: LINES 64599 TO 64639
// ----------------------------------------
L64599  | 			}
L64600  | 			base.Complete(network);
L64601  | 		}
L64602  | 
L64603  | 		public void SetDealer(Dealer dealer)
L64604  | 		{
L64605  | 			Dealer = dealer;
L64606  | 			if (dealer != null)
L64607  | 			{
L64608  | 				ShouldSendExpiryReminder = false;
L64609  | 				ShouldSendExpiredNotification = false;
L64610  | 			}
L64611  | 			if (journalEntry != null)
L64612  | 			{
L64613  | 				journalEntry.gameObject.SetActive(ShouldShowJournalEntry());
L64614  | 			}
L64615  | 		}
L64616  | 
L64617  | 		public virtual void SubmitPayment(float bonusTotal)
L64618  | 		{
L64619  | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(Payment + bonusTotal);
L64620  | 		}
L64621  | 
L64622  | 		protected override void SendExpiryReminder()
L64623  | 		{
L64624  | 			Singleton<NotificationsManager>.Instance.SendNotification("<color=#FFB43C>Deal Expiring Soon</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
L64625  | 		}
L64626  | 
L64627  | 		protected override void SendExpiredNotification()
L64628  | 		{
L64629  | 			Singleton<NotificationsManager>.Instance.SendNotification("<color=#FF6455>Deal Expired</color>", title, PlayerSingleton<JournalApp>.Instance.AppIcon);
L64630  | 		}
L64631  | 
L64632  | 		protected override bool ShouldShowJournalEntry()
L64633  | 		{
L64634  | 			if (Dealer != null)
L64635  | 			{
L64636  | 				return false;
L64637  | 			}
L64638  | 			return base.ShouldShowJournalEntry();
L64639  | 		}

// <<< END OF BLOCK: LINES 64599 TO 64639





// ################################################################################





// >>> START OF BLOCK: LINES 64791 TO 64831
// ----------------------------------------
L64791  | 			if (GUIDManager.IsGUIDValid(deliveryLocationGUID))
L64792  | 			{
L64793  | 				DeliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(deliveryLocationGUID));
L64794  | 			}
L64795  | 		}
L64796  | 
L64797  | 		public ContractInfo()
L64798  | 		{
L64799  | 		}
L64800  | 
L64801  | 		public DialogueChain ProcessMessage(DialogueChain messageChain)
L64802  | 		{
L64803  | 			if (DeliveryLocation == null && GUIDManager.IsGUIDValid(DeliveryLocationGUID))
L64804  | 			{
L64805  | 				DeliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(DeliveryLocationGUID));
L64806  | 			}
L64807  | 			List<string> list = new List<string>();
L64808  | 			string[] lines = messageChain.Lines;
L64809  | 			for (int i = 0; i < lines.Length; i++)
L64810  | 			{
L64811  | 				string text = lines[i].Replace("<PRICE>", "<color=#46CB4F>" + MoneyManager.FormatAmount(Payment) + "</color>");
L64812  | 				text = text.Replace("<PRODUCT>", Products.GetCommaSeperatedString());
L64813  | 				text = text.Replace("<QUALITY>", Products.GetQualityString());
L64814  | 				text = text.Replace("<LOCATION>", "<b>" + DeliveryLocation.GetDescription() + "</b>");
L64815  | 				text = text.Replace("<WINDOW_START>", TimeManager.Get12HourTime(DeliveryWindow.WindowStartTime));
L64816  | 				text = text.Replace("<WINDOW_END>", TimeManager.Get12HourTime(DeliveryWindow.WindowEndTime));
L64817  | 				list.Add(text);
L64818  | 			}
L64819  | 			return new DialogueChain
L64820  | 			{
L64821  | 				Lines = list.ToArray()
L64822  | 			};
L64823  | 		}
L64824  | 	}
L64825  | 	public enum EQuestState
L64826  | 	{
L64827  | 		Inactive,
L64828  | 		Active,
L64829  | 		Completed,
L64830  | 		Failed,
L64831  | 		Expired,

// <<< END OF BLOCK: LINES 64791 TO 64831





// ################################################################################





// >>> START OF BLOCK: LINES 66000 TO 66040
// ----------------------------------------
L66000  | 		{
L66001  | 			if (!InstanceFinder.IsServer)
L66002  | 			{
L66003  | 				Console.LogError("SendContractAccepted can only be called on the server!");
L66004  | 				return null;
L66005  | 			}
L66006  | 			GameDateTime expiry = new GameDateTime
L66007  | 			{
L66008  | 				time = contractData.DeliveryWindow.WindowEndTime,
L66009  | 				elapsedDays = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.ElapsedDays
L66010  | 			};
L66011  | 			if (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentTime > contractData.DeliveryWindow.WindowEndTime)
L66012  | 			{
L66013  | 				expiry.elapsedDays++;
L66014  | 			}
L66015  | 			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Accepted_Contract_Count", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Accepted_Contract_Count") + 1f).ToString());
L66016  | 			string nameAddress = customer.NPC.GetNameAddress();
L66017  | 			GameDateTime dateTime = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime();
L66018  | 			DeliveryLocation deliveryLocation = GUIDManager.GetObject<DeliveryLocation>(new Guid(contractData.DeliveryLocationGUID));
L66019  | 			string title = "Deal for " + nameAddress;
L66020  | 			string description = nameAddress + " has requested a delivery of " + contractData.Products.GetCommaSeperatedString() + " " + deliveryLocation.GetDescription() + " for " + MoneyManager.FormatAmount(contractData.Payment) + ".";
L66021  | 			QuestEntryData questEntryData = new QuestEntryData(contractData.Products.GetCommaSeperatedString() + ", " + deliveryLocation.LocationName, EQuestState.Inactive);
L66022  | 			Contract result = CreateContract_Local(title, description, new QuestEntryData[1] { questEntryData }, guid, track, customer, contractData.Payment, contractData.Products, contractData.DeliveryLocationGUID, contractData.DeliveryWindow, contractData.Expires, expiry, contractData.PickupScheduleIndex, dateTime, dealer);
L66023  | 			CreateContract_Networked(dealerObj: (dealer != null) ? dealer.NetworkObject : null, conn: null, title: title, description: description, guid: guid, tracked: track, customer: customer.NetworkObject, contractData: contractData, expiry: expiry, acceptTime: dateTime);
L66024  | 			return result;
L66025  | 		}
L66026  | 
L66027  | 		[ObserversRpc(RunLocally = true)]
L66028  | 		[TargetRpc]
L66029  | 		public void CreateContract_Networked(NetworkConnection conn, string title, string description, string guid, bool tracked, NetworkObject customer, ContractInfo contractData, GameDateTime expiry, GameDateTime acceptTime, NetworkObject dealerObj = null)
L66030  | 		{
L66031  | 			if ((object)conn == null)
L66032  | 			{
L66033  | 				RpcWriter___Observers_CreateContract_Networked_2526053753(conn, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
L66034  | 				RpcLogic___CreateContract_Networked_2526053753(conn, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
L66035  | 			}
L66036  | 			else
L66037  | 			{
L66038  | 				RpcWriter___Target_CreateContract_Networked_2526053753(conn, title, description, guid, tracked, customer, contractData, expiry, acceptTime, dealerObj);
L66039  | 			}
L66040  | 		}

// <<< END OF BLOCK: LINES 66000 TO 66040





// ################################################################################





// >>> START OF BLOCK: LINES 67564 TO 67605
// ----------------------------------------
L67564  | 				int count = Customer.UnlockedCustomers.Count;
L67565  | 				ReachCustomersEntry.SetEntryTitle("Unlock 10 customers (" + count + "/10)");
L67566  | 				if (count >= 10 && ReachCustomersEntry.State != EQuestState.Completed)
L67567  | 				{
L67568  | 					ReachCustomersEntry.Complete();
L67569  | 				}
L67570  | 			}
L67571  | 		}
L67572  | 	}
L67573  | 	public class Quest_NeedingTheGreen : Quest
L67574  | 	{
L67575  | 		public Quest[] PrerequisiteQuests;
L67576  | 
L67577  | 		public QuestEntry EarnEntry;
L67578  | 
L67579  | 		public float LifetimeEarningsRequirement = 10000f;
L67580  | 
L67581  | 		protected override void MinPass()
L67582  | 		{
L67583  | 			base.MinPass();
L67584  | 			string text = MoneyManager.FormatAmount(LifetimeEarningsRequirement);
L67585  | 			EarnEntry.SetEntryTitle("Earn " + text + " (" + MoneyManager.FormatAmount(NetworkSingleton<MoneyManager>.Instance.LifetimeEarnings) + " / " + text + ")");
L67586  | 			if (!InstanceFinder.IsServer || base.State != EQuestState.Inactive)
L67587  | 			{
L67588  | 				return;
L67589  | 			}
L67590  | 			Quest[] prerequisiteQuests = PrerequisiteQuests;
L67591  | 			for (int i = 0; i < prerequisiteQuests.Length; i++)
L67592  | 			{
L67593  | 				if (prerequisiteQuests[i].State != EQuestState.Completed)
L67594  | 				{
L67595  | 					return;
L67596  | 				}
L67597  | 			}
L67598  | 			Begin();
L67599  | 		}
L67600  | 	}
L67601  | 	public class Quest_OnTheGrind : Quest
L67602  | 	{
L67603  | 		public QuestEntry CompleteDealsEntry;
L67604  | 
L67605  | 		protected override void MinPass()

// <<< END OF BLOCK: LINES 67564 TO 67605





// ################################################################################





// >>> START OF BLOCK: LINES 68378 TO 68418
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

// <<< END OF BLOCK: LINES 68378 TO 68418





// ################################################################################





// >>> START OF BLOCK: LINES 68489 TO 68533
// ----------------------------------------
L68489  | 		}
L68490  | 
L68491  | 		[TargetRpc]
L68492  | 		[ObserversRpc]
L68493  | 		private void ReceiveLaunderingOperation(NetworkConnection conn, float amount, int minutesSinceStarted = 0)
L68494  | 		{
L68495  | 			if ((object)conn == null)
L68496  | 			{
L68497  | 				RpcWriter___Observers_ReceiveLaunderingOperation_1001022388(conn, amount, minutesSinceStarted);
L68498  | 			}
L68499  | 			else
L68500  | 			{
L68501  | 				RpcWriter___Target_ReceiveLaunderingOperation_1001022388(conn, amount, minutesSinceStarted);
L68502  | 			}
L68503  | 		}
L68504  | 
L68505  | 		protected void CompleteOperation(LaunderingOperation op)
L68506  | 		{
L68507  | 			if (InstanceFinder.IsServer)
L68508  | 			{
L68509  | 				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Money laundering (" + propertyName + ")", op.amount, 1f, string.Empty);
L68510  | 				float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("LaunderingOperationsCompleted");
L68511  | 				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LaunderingOperationsCompleted", (value + 1f).ToString());
L68512  | 			}
L68513  | 			Singleton<NotificationsManager>.Instance.SendNotification(propertyName, "<color=#16F01C>" + MoneyManager.FormatAmount(op.amount) + "</color> Laundered", NetworkSingleton<MoneyManager>.Instance.LaunderingNotificationIcon);
L68514  | 			LaunderingOperations.Remove(op);
L68515  | 			base.HasChanged = true;
L68516  | 			if (onOperationFinished != null)
L68517  | 			{
L68518  | 				onOperationFinished(op);
L68519  | 			}
L68520  | 		}
L68521  | 
L68522  | 		public override void NetworkInitialize___Early()
L68523  | 		{
L68524  | 			if (!NetworkInitialize___EarlyScheduleOne.Property.BusinessAssembly-CSharp.dll_Excuted)
L68525  | 			{
L68526  | 				NetworkInitialize___EarlyScheduleOne.Property.BusinessAssembly-CSharp.dll_Excuted = true;
L68527  | 				base.NetworkInitialize___Early();
L68528  | 				RegisterServerRpc(5u, RpcReader___Server_StartLaunderingOperation_1481775633);
L68529  | 				RegisterTargetRpc(6u, RpcReader___Target_ReceiveLaunderingOperation_1001022388);
L68530  | 				RegisterObserversRpc(7u, RpcReader___Observers_ReceiveLaunderingOperation_1001022388);
L68531  | 			}
L68532  | 		}
L68533  | 

// <<< END OF BLOCK: LINES 68489 TO 68533





// ################################################################################





// >>> START OF BLOCK: LINES 69421 TO 69462
// ----------------------------------------
L69421  | 		public List<string> LocalExtraFiles { get; set; } = new List<string>();
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

// <<< END OF BLOCK: LINES 69421 TO 69462





// ################################################################################





// >>> START OF BLOCK: LINES 69484 TO 69534
// ----------------------------------------
L69484  | 		}
L69485  | 
L69486  | 		public override void OnSpawnServer(NetworkConnection connection)
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

// <<< END OF BLOCK: LINES 69484 TO 69534





// ################################################################################





// >>> START OF BLOCK: LINES 70047 TO 70101
// ----------------------------------------
L70047  | 				RpcLogic___SetToggleableState_338960014(base.LocalConnection, index, state);
L70048  | 			}
L70049  | 		}
L70050  | 
L70051  | 		protected virtual void Awake_UserLogic_ScheduleOne.Property.Property_Assembly-CSharp.dll()
L70052  | 		{
L70053  | 			propertyBoundsColliders = BoundingBox.GetComponentsInChildren<BoxCollider>();
L70054  | 			BoxCollider[] array = propertyBoundsColliders;
L70055  | 			foreach (BoxCollider obj in array)
L70056  | 			{
L70057  | 				obj.isTrigger = true;
L70058  | 				obj.gameObject.layer = LayerMask.NameToLayer("Invisible");
L70059  | 			}
L70060  | 			Properties.Add(this);
L70061  | 			UnownedProperties.Remove(this);
L70062  | 			UnownedProperties.Add(this);
L70063  | 			Container.SetProperty(this);
L70064  | 			PoI.SetMainText(propertyName + " (Unowned)");
L70065  | 			SetBoundsVisible(vis: false);
L70066  | 			ForSaleSign.transform.Find("Name").GetComponent<TextMeshPro>().text = propertyName;
L70067  | 			ForSaleSign.transform.Find("Price").GetComponent<TextMeshPro>().text = MoneyManager.FormatAmount(Price);
L70068  | 			if (EmployeeIdlePoints.Length < EmployeeCapacity)
L70069  | 			{
L70070  | 				UnityEngine.Debug.LogWarning("Property " + PropertyName + " has less idle points than employee capacity.");
L70071  | 			}
L70072  | 			if (!GameManager.IS_TUTORIAL)
L70073  | 			{
L70074  | 				WorldspaceUIContainer = new GameObject(propertyName + " Worldspace UI Container").AddComponent<RectTransform>();
L70075  | 				WorldspaceUIContainer.SetParent(Singleton<ManagementWorldspaceCanvas>.Instance.Canvas.transform);
L70076  | 				WorldspaceUIContainer.gameObject.SetActive(value: false);
L70077  | 			}
L70078  | 			if (ListingPoster != null)
L70079  | 			{
L70080  | 				ListingPoster.Find("Title").GetComponent<TextMeshPro>().text = propertyName;
L70081  | 				ListingPoster.Find("Price").GetComponent<TextMeshPro>().text = MoneyManager.FormatAmount(Price);
L70082  | 				ListingPoster.Find("Parking/Text").GetComponent<TextMeshPro>().text = LoadingDockCount.ToString();
L70083  | 				ListingPoster.Find("Employee/Text").GetComponent<TextMeshPro>().text = EmployeeCapacity.ToString();
L70084  | 			}
L70085  | 			PoI.gameObject.SetActive(value: false);
L70086  | 			foreach (ModularSwitch @switch in Switches)
L70087  | 			{
L70088  | 				if (!(@switch == null))
L70089  | 				{
L70090  | 					@switch.onToggled = (ModularSwitch.ButtonChange)Delegate.Combine(@switch.onToggled, (ModularSwitch.ButtonChange)delegate
L70091  | 					{
L70092  | 						HasChanged = true;
L70093  | 					});
L70094  | 				}
L70095  | 			}
L70096  | 			foreach (InteractableToggleable toggleable2 in Toggleables)
L70097  | 			{
L70098  | 				if (!(toggleable2 == null))
L70099  | 				{
L70100  | 					InteractableToggleable toggleable1 = toggleable2;
L70101  | 					toggleable2.onToggle.AddListener(delegate

// <<< END OF BLOCK: LINES 70047 TO 70101





// ################################################################################





// >>> START OF BLOCK: LINES 80070 TO 80110
// ----------------------------------------
L80070  | 			}
L80071  | 		}
L80072  | 	}
L80073  | 	public class MoneyLoader : Loader
L80074  | 	{
L80075  | 		public override void Load(string mainPath)
L80076  | 		{
L80077  | 			if (TryLoadFile(mainPath, out var contents))
L80078  | 			{
L80079  | 				MoneyData moneyData = null;
L80080  | 				try
L80081  | 				{
L80082  | 					moneyData = JsonUtility.FromJson<MoneyData>(contents);
L80083  | 				}
L80084  | 				catch (Exception ex)
L80085  | 				{
L80086  | 					Console.LogError(GetType()?.ToString() + " error reading data: " + ex);
L80087  | 				}
L80088  | 				if (moneyData != null)
L80089  | 				{
L80090  | 					NetworkSingleton<MoneyManager>.Instance.Load(moneyData);
L80091  | 				}
L80092  | 			}
L80093  | 		}
L80094  | 	}
L80095  | 	public class NPCsLoader : Loader
L80096  | 	{
L80097  | 		public virtual string NPCType => typeof(NPCCollectionData).Name;
L80098  | 
L80099  | 		public override void Load(string mainPath)
L80100  | 		{
L80101  | 			NPCLoader nPCLoader = new NPCLoader();
L80102  | 			bool flag = false;
L80103  | 			if (TryLoadFile(mainPath, out var contents))
L80104  | 			{
L80105  | 				NPCCollectionData nPCCollectionData = null;
L80106  | 				try
L80107  | 				{
L80108  | 					nPCCollectionData = JsonUtility.FromJson<NPCCollectionData>(contents);
L80109  | 				}
L80110  | 				catch (Exception ex)

// <<< END OF BLOCK: LINES 80070 TO 80110





// ################################################################################





// >>> START OF BLOCK: LINES 96880 TO 96934
// ----------------------------------------
L96880  | 				}
L96881  | 				else if (array[i] is Vandalism)
L96882  | 				{
L96883  | 					num += 50f;
L96884  | 				}
L96885  | 				else if (array[i] is Theft)
L96886  | 				{
L96887  | 					num += 50f;
L96888  | 				}
L96889  | 				else if (array[i] is BrandishingWeapon)
L96890  | 				{
L96891  | 					num += 50f;
L96892  | 				}
L96893  | 				else if (array[i] is DischargeFirearm)
L96894  | 				{
L96895  | 					num += 50f;
L96896  | 				}
L96897  | 			}
L96898  | 			if (num > 0f)
L96899  | 			{
L96900  | 				float num6 = Mathf.Min(num, NetworkSingleton<MoneyManager>.Instance.cashBalance);
L96901  | 				string text = MoneyManager.FormatAmount(num, showDecimals: true) + " fine";
L96902  | 				if (num6 == num)
L96903  | 				{
L96904  | 					text += " (paid in cash)";
L96905  | 				}
L96906  | 				else
L96907  | 				{
L96908  | 					text = text + " (" + MoneyManager.FormatAmount(num6, showDecimals: true) + " paid";
L96909  | 					text += " - insufficient cash)";
L96910  | 				}
L96911  | 				list.Add(text);
L96912  | 				if (num6 > 0f)
L96913  | 				{
L96914  | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - num6);
L96915  | 				}
L96916  | 			}
L96917  | 			return list;
L96918  | 		}
L96919  | 	}
L96920  | 	[Serializable]
L96921  | 	public class SentryInstance
L96922  | 	{
L96923  | 		public SentryLocation Location;
L96924  | 
L96925  | 		public int Members = 2;
L96926  | 
L96927  | 		[Header("Timing")]
L96928  | 		public int StartTime = 2000;
L96929  | 
L96930  | 		public int EndTime = 100;
L96931  | 
L96932  | 		[Range(1f, 10f)]
L96933  | 		public int IntensityRequirement = 5;
L96934  | 

// <<< END OF BLOCK: LINES 96880 TO 96934





// ################################################################################





// >>> START OF BLOCK: LINES 97873 TO 97929
// ----------------------------------------
L97873  | 
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

// <<< END OF BLOCK: LINES 97873 TO 97929





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





// >>> START OF BLOCK: LINES 116906 TO 116946
// ----------------------------------------
L116906 | 				NameLabel.text = AssignedEmployee.FirstName + "\n" + AssignedEmployee.LastName;
L116907 | 				Clipboard.gameObject.SetActive(value: true);
L116908 | 			}
L116909 | 			else
L116910 | 			{
L116911 | 				Clipboard.gameObject.SetActive(value: false);
L116912 | 			}
L116913 | 			if (onAssignedEmployeeChanged != null)
L116914 | 			{
L116915 | 				onAssignedEmployeeChanged.Invoke();
L116916 | 			}
L116917 | 			UpdateStorageText();
L116918 | 			UpdateMaterial();
L116919 | 		}
L116920 | 
L116921 | 		private void UpdateStorageText()
L116922 | 		{
L116923 | 			if (AssignedEmployee != null)
L116924 | 			{
L116925 | 				Storage.StorageEntityName = AssignedEmployee.FirstName + "'s " + HomeType;
L116926 | 				string text = "<color=#54E717>" + MoneyManager.FormatAmount(AssignedEmployee.DailyWage) + "</color>";
L116927 | 				Storage.StorageEntitySubtitle = AssignedEmployee.fullName + " will draw " + (AssignedEmployee.IsMale ? "his" : "her") + " daily wage of " + text + " from this " + HomeType.ToLower();
L116928 | 			}
L116929 | 			else
L116930 | 			{
L116931 | 				Storage.StorageEntityName = HomeType;
L116932 | 				Storage.StorageEntitySubtitle = string.Empty;
L116933 | 			}
L116934 | 		}
L116935 | 
L116936 | 		private void UpdateMaterial()
L116937 | 		{
L116938 | 			MeshRenderer[] employeeSpecificMeshes = EmployeeSpecificMeshes;
L116939 | 			foreach (MeshRenderer meshRenderer in employeeSpecificMeshes)
L116940 | 			{
L116941 | 				if (AssignedEmployee != null)
L116942 | 				{
L116943 | 					switch (AssignedEmployee.EmployeeType)
L116944 | 					{
L116945 | 					case EEmployeeType.Botanist:
L116946 | 						meshRenderer.material = SpecificMat_Botanist;

// <<< END OF BLOCK: LINES 116906 TO 116946





// ################################################################################





// >>> START OF BLOCK: LINES 120116 TO 120156
// ----------------------------------------
L120116 | 				return;
L120117 | 			}
L120118 | 			ProductDefinition item = Registry.GetItem<ProductDefinition>(OfferedContractInfo.Products.entries[0].ProductID);
L120119 | 			int quantity = OfferedContractInfo.Products.entries[0].Quantity;
L120120 | 			float payment = OfferedContractInfo.Payment;
L120121 | 			PlayerSingleton<MessagesApp>.Instance.CounterofferInterface.Open(item, quantity, payment, NPC.MSGConversation, SendCounteroffer);
L120122 | 		}
L120123 | 
L120124 | 		protected virtual void SendCounteroffer(ProductDefinition product, int quantity, float price)
L120125 | 		{
L120126 | 			if (OfferedContractInfo == null)
L120127 | 			{
L120128 | 				Console.LogWarning("Offered contract is null!");
L120129 | 				return;
L120130 | 			}
L120131 | 			if (OfferedContractInfo.IsCounterOffer)
L120132 | 			{
L120133 | 				Console.LogWarning("Counter offer already sent");
L120134 | 				return;
L120135 | 			}
L120136 | 			string text = "How about " + quantity + "x " + product.Name + " for " + MoneyManager.FormatAmount(price) + "?";
L120137 | 			NPC.MSGConversation.SendMessage(new ScheduleOne.Messaging.Message(text, ScheduleOne.Messaging.Message.ESenderType.Player));
L120138 | 			NPC.MSGConversation.ClearResponses();
L120139 | 			ProcessCounterOfferServerSide(product.ID, quantity, price);
L120140 | 		}
L120141 | 
L120142 | 		[ServerRpc(RequireOwnership = false)]
L120143 | 		private void ProcessCounterOfferServerSide(string productID, int quantity, float price)
L120144 | 		{
L120145 | 			RpcWriter___Server_ProcessCounterOfferServerSide_900355577(productID, quantity, price);
L120146 | 		}
L120147 | 
L120148 | 		[ObserversRpc(RunLocally = true)]
L120149 | 		private void SetContractIsCounterOffer()
L120150 | 		{
L120151 | 			RpcWriter___Observers_SetContractIsCounterOffer_2166136261();
L120152 | 			RpcLogic___SetContractIsCounterOffer_2166136261();
L120153 | 		}
L120154 | 
L120155 | 		protected virtual void PlayerAcceptedContract(EDealWindow window)
L120156 | 		{

// <<< END OF BLOCK: LINES 120116 TO 120156





// ################################################################################





// >>> START OF BLOCK: LINES 122095 TO 122135
// ----------------------------------------
L122095 | 					NetworkSingleton<DailySummary>.Instance.AddSoldItem(list[i], list2[i]);
L122096 | 				}
L122097 | 				NetworkSingleton<DailySummary>.Instance.AddPlayerMoney(totalPayment);
L122098 | 				NetworkSingleton<LevelManager>.Instance.AddXP(20);
L122099 | 			}
L122100 | 			else
L122101 | 			{
L122102 | 				if (flag)
L122103 | 				{
L122104 | 					NetworkSingleton<LevelManager>.Instance.AddXP(10);
L122105 | 					NetworkSingleton<DailySummary>.Instance.AddDealerMoney(totalPayment);
L122106 | 				}
L122107 | 				if (dealer != null)
L122108 | 				{
L122109 | 					dealer.CompletedDeal();
L122110 | 					dealer.SubmitPayment(totalPayment);
L122111 | 				}
L122112 | 			}
L122113 | 			if (flag)
L122114 | 			{
L122115 | 				NetworkSingleton<MoneyManager>.Instance.ChangeLifetimeEarnings(totalPayment);
L122116 | 			}
L122117 | 			NPC.Inventory.RemoveCash(totalPayment);
L122118 | 			if (CurrentContract != null)
L122119 | 			{
L122120 | 				CurrentContract.Complete();
L122121 | 			}
L122122 | 			List<StringIntPair> list3 = new List<StringIntPair>();
L122123 | 			foreach (ItemInstance item in items)
L122124 | 			{
L122125 | 				NPC.Inventory.InsertItem(item);
L122126 | 				list3.Add(new StringIntPair(item.ID, item.Quantity));
L122127 | 			}
L122128 | 			EContractParty completedBy = EContractParty.Player;
L122129 | 			if (!handoverByPlayer && dealer != null)
L122130 | 			{
L122131 | 				completedBy = ((dealer.DealerType != EDealerType.CartelDealer) ? EContractParty.PlayerDealer : EContractParty.Cartel);
L122132 | 			}
L122133 | 			ContractReceipt receipt = new ContractReceipt(UnityEngine.Random.Range(int.MinValue, int.MaxValue), completedBy, NPC.ID, NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.GetDateTime(), list3.ToArray(), totalPayment);
L122134 | 			NetworkSingleton<ProductManager>.Instance.RecordContractReceipt(receipt);
L122135 | 			if (items.Count > 0)

// <<< END OF BLOCK: LINES 122095 TO 122135





// ################################################################################





// >>> START OF BLOCK: LINES 123229 TO 123330
// ----------------------------------------
L123229 | 		[ObserversRpc(RunLocally = true)]
L123230 | 		[TargetRpc]
L123231 | 		public virtual void SetIsRecruited(NetworkConnection conn)
L123232 | 		{
L123233 | 			if ((object)conn == null)
L123234 | 			{
L123235 | 				RpcWriter___Observers_SetIsRecruited_328543758(conn);
L123236 | 				RpcLogic___SetIsRecruited_328543758(conn);
L123237 | 			}
L123238 | 			else
L123239 | 			{
L123240 | 				RpcWriter___Target_SetIsRecruited_328543758(conn);
L123241 | 			}
L123242 | 		}
L123243 | 
L123244 | 		protected virtual void OnDealerUnlocked(NPCRelationData.EUnlockType unlockType, bool b)
L123245 | 		{
L123246 | 			UpdatePotentialDealerPoI();
L123247 | 			if (!Singleton<LoadManager>.Instance.IsLoading)
L123248 | 			{
L123249 | 				NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
L123250 | 			}
L123251 | 		}
L123252 | 
L123253 | 		protected virtual void UpdatePotentialDealerPoI()
L123254 | 		{
L123255 | 			if (PotentialDealerPoI != null)
L123256 | 			{
L123257 | 				PotentialDealerPoI.enabled = RelationData.IsMutuallyKnown() && !RelationData.Unlocked;
L123258 | 			}
L123259 | 		}
L123260 | 
L123261 | 		private void DealerUnconscious()
L123262 | 		{
L123263 | 			List<Contract> list = new List<Contract>(ActiveContracts);
L123264 | 			for (int i = 0; i < list.Count; i++)
L123265 | 			{
L123266 | 				list[i].Fail();
L123267 | 			}
L123268 | 		}
L123269 | 
L123270 | 		private void TradeItems()
L123271 | 		{
L123272 | 			DialogueHandler.SkipNextDialogueBehaviourEnd();
L123273 | 			itemCountOnTradeStart = ((IItemSlotOwner)Inventory).GetQuantitySum();
L123274 | 			Singleton<StorageMenu>.Instance.Open(Inventory, base.fullName + "'s Inventory", "Place <color=#4CB0FF>packaged product</color> here and the dealer will sell it to assigned customers");
L123275 | 			Singleton<StorageMenu>.Instance.onClosed.AddListener(TradeItemsDone);
L123276 | 		}
L123277 | 
L123278 | 		private void TradeItemsDone()
L123279 | 		{
L123280 | 			Singleton<StorageMenu>.Instance.onClosed.RemoveListener(TradeItemsDone);
L123281 | 			Behaviour.GenericDialogueBehaviour.Disable_Server();
L123282 | 			if (((IItemSlotOwner)Inventory).GetQuantitySum() > itemCountOnTradeStart)
L123283 | 			{
L123284 | 				DialogueHandler.WorldspaceRend.ShowText("Thanks boss", 2.5f);
L123285 | 				PlayVO(EVOLineType.Thanks);
L123286 | 			}
L123287 | 			TryMoveOverflowItems();
L123288 | 		}
L123289 | 
L123290 | 		private bool CanCollectCash(out string reason)
L123291 | 		{
L123292 | 			reason = string.Empty;
L123293 | 			if (SyncAccessor_<Cash>k__BackingField <= 0f)
L123294 | 			{
L123295 | 				return false;
L123296 | 			}
L123297 | 			return true;
L123298 | 		}
L123299 | 
L123300 | 		private void UpdateCollectCashChoice(float oldCash, float newCash, bool asServer)
L123301 | 		{
L123302 | 			if (collectCashChoice != null)
L123303 | 			{
L123304 | 				collectCashChoice.ChoiceText = "I need to collect the earnings <color=#54E717>(" + MoneyManager.FormatAmount(SyncAccessor_<Cash>k__BackingField) + ")</color>";
L123305 | 			}
L123306 | 		}
L123307 | 
L123308 | 		private void CollectCash()
L123309 | 		{
L123310 | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(SyncAccessor_<Cash>k__BackingField, visualizeChange: true, playCashSound: true);
L123311 | 			SetCash(0f);
L123312 | 		}
L123313 | 
L123314 | 		private void CheckCurrentDealValidity()
L123315 | 		{
L123316 | 			if (currentContract.State != EQuestState.Active)
L123317 | 			{
L123318 | 				_attendDealBehaviour.Disable_Server();
L123319 | 				currentContract.SetDealer(null);
L123320 | 				currentContract = null;
L123321 | 			}
L123322 | 		}
L123323 | 
L123324 | 		private bool CanOfferRecruitment(out string reason)
L123325 | 		{
L123326 | 			reason = string.Empty;
L123327 | 			if (IsRecruited)
L123328 | 			{
L123329 | 				return false;
L123330 | 			}

// <<< END OF BLOCK: LINES 123229 TO 123330





// ################################################################################





// >>> START OF BLOCK: LINES 123544 TO 123584
// ----------------------------------------
L123544 | 			{
L123545 | 				if (items.Count != 0 || !(cash <= 0f))
L123546 | 				{
L123547 | 					List<string> list3 = new List<string>();
L123548 | 					for (int k = 0; k < items.Count; k++)
L123549 | 					{
L123550 | 						string text = items[k].Quantity + "x ";
L123551 | 						if (items[k] is ProductItemInstance && (items[k] as ProductItemInstance).AppliedPackaging != null)
L123552 | 						{
L123553 | 							text = text + (items[k] as ProductItemInstance).AppliedPackaging.Name + " of ";
L123554 | 						}
L123555 | 						text += items[k].Definition.Name;
L123556 | 						if (items[k] is QualityItemInstance)
L123557 | 						{
L123558 | 							text = text + " (" + (items[k] as QualityItemInstance).Quality.ToString() + " quality)";
L123559 | 						}
L123560 | 						list3.Add(text);
L123561 | 					}
L123562 | 					if (cash > 0f)
L123563 | 					{
L123564 | 						list3.Add(MoneyManager.FormatAmount(cash) + " cash");
L123565 | 					}
L123566 | 					string text2 = "This is what they got:\n" + string.Join("\n", list3);
L123567 | 					base.MSGConversation.SendMessage(new ScheduleOne.Messaging.Message(text2, ScheduleOne.Messaging.Message.ESenderType.Other, _endOfGroup: true), notify: false);
L123568 | 				}
L123569 | 			}
L123570 | 		}
L123571 | 
L123572 | 		public List<ProductDefinition> GetOrderableProducts()
L123573 | 		{
L123574 | 			List<ProductDefinition> list = new List<ProductDefinition>();
L123575 | 			foreach (ItemSlot allSlot in GetAllSlots())
L123576 | 			{
L123577 | 				if (allSlot.ItemInstance != null && allSlot.ItemInstance is ProductItemInstance)
L123578 | 				{
L123579 | 					ProductItemInstance product = allSlot.ItemInstance as ProductItemInstance;
L123580 | 					if (list.Find((ProductDefinition x) => x.ID == product.ID) == null)
L123581 | 					{
L123582 | 						list.Add(product.Definition as ProductDefinition);
L123583 | 					}
L123584 | 				}

// <<< END OF BLOCK: LINES 123544 TO 123584





// ################################################################################





// >>> START OF BLOCK: LINES 124754 TO 124794
// ----------------------------------------
L124754 | 			}
L124755 | 			else
L124756 | 			{
L124757 | 				Channel channel = Channel.Reliable;
L124758 | 				PooledWriter writer = WriterPool.GetWriter();
L124759 | 				writer.WriteSingle(payment);
L124760 | 				SendServerRpc(52u, writer, channel, DataOrderType.Default);
L124761 | 				writer.Store();
L124762 | 			}
L124763 | 		}
L124764 | 
L124765 | 		public void RpcLogic___SubmitPayment_431000436(float payment)
L124766 | 		{
L124767 | 			if (!(payment <= 0f))
L124768 | 			{
L124769 | 				Console.Log("Dealer " + base.fullName + " received payment: " + payment);
L124770 | 				float num = SyncAccessor_<Cash>k__BackingField;
L124771 | 				ChangeCash(payment * (1f - Cut));
L124772 | 				if (InstanceFinder.IsServer && DealerType == EDealerType.PlayerDealer && SyncAccessor_<Cash>k__BackingField >= 500f && num < 500f)
L124773 | 				{
L124774 | 					base.MSGConversation.SendMessage(new ScheduleOne.Messaging.Message("Hey boss, just letting you know I've got " + MoneyManager.FormatAmount(SyncAccessor_<Cash>k__BackingField) + " ready for you to collect.", ScheduleOne.Messaging.Message.ESenderType.Other, _endOfGroup: true));
L124775 | 				}
L124776 | 			}
L124777 | 		}
L124778 | 
L124779 | 		private void RpcReader___Server_SubmitPayment_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
L124780 | 		{
L124781 | 			float payment = PooledReader0.ReadSingle();
L124782 | 			if (base.IsServerInitialized)
L124783 | 			{
L124784 | 				RpcLogic___SubmitPayment_431000436(payment);
L124785 | 			}
L124786 | 		}
L124787 | 
L124788 | 		private void RpcWriter___Server_SetStoredInstance_2652194801(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
L124789 | 		{
L124790 | 			if (!base.IsClientInitialized)
L124791 | 			{
L124792 | 				NetworkManager networkManager = base.NetworkManager;
L124793 | 				if ((object)networkManager == null)
L124794 | 				{

// <<< END OF BLOCK: LINES 124754 TO 124794





// ################################################################################





// >>> START OF BLOCK: LINES 125420 TO 125460
// ----------------------------------------
L125420 | 			{
L125421 | 				return false;
L125422 | 			}
L125423 | 			if (!NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(GenerationTime, TimeManager.AddMinutesTo24HourTime(GenerationTime, GenerationWindowDuration)))
L125424 | 			{
L125425 | 				return false;
L125426 | 			}
L125427 | 			return true;
L125428 | 		}
L125429 | 
L125430 | 		public MessageChain GetRandomRequestMessage()
L125431 | 		{
L125432 | 			return ProcessMessage(RequestMessageChains[UnityEngine.Random.Range(0, RequestMessageChains.Length - 1)]);
L125433 | 		}
L125434 | 
L125435 | 		public MessageChain ProcessMessage(MessageChain messageChain)
L125436 | 		{
L125437 | 			MessageChain messageChain2 = new MessageChain();
L125438 | 			foreach (string message in messageChain.Messages)
L125439 | 			{
L125440 | 				string text = message.Replace("<PRICE>", "<color=#46CB4F>" + MoneyManager.FormatAmount(Payment) + "</color>");
L125441 | 				text = text.Replace("<PRODUCT>", GetProductStringList());
L125442 | 				text = text.Replace("<QUALITY>", GetQualityString());
L125443 | 				text = text.Replace("<LOCATION>", DeliveryLocation.GetDescription());
L125444 | 				text = text.Replace("<WINDOW_START>", TimeManager.Get12HourTime(DeliveryWindow.WindowStartTime));
L125445 | 				text = text.Replace("<WINDOW_END>", TimeManager.Get12HourTime(DeliveryWindow.WindowEndTime));
L125446 | 				messageChain2.Messages.Add(text);
L125447 | 			}
L125448 | 			return messageChain2;
L125449 | 		}
L125450 | 
L125451 | 		public MessageChain GetRejectionMessage()
L125452 | 		{
L125453 | 			return ProcessMessage(ContractRejectedResponses[UnityEngine.Random.Range(0, ContractRejectedResponses.Length - 1)]);
L125454 | 		}
L125455 | 
L125456 | 		public MessageChain GetAcceptanceMessage()
L125457 | 		{
L125458 | 			return ProcessMessage(ContractAcceptedResponses[UnityEngine.Random.Range(0, ContractAcceptedResponses.Length - 1)]);
L125459 | 		}
L125460 | 

// <<< END OF BLOCK: LINES 125420 TO 125460





// ################################################################################





// >>> START OF BLOCK: LINES 126081 TO 126122
// ----------------------------------------
L126081 | 		}
L126082 | 
L126083 | 		[ServerRpc(RequireOwnership = false, RunLocally = true)]
L126084 | 		private void ChangeDebt(float amount)
L126085 | 		{
L126086 | 			RpcWriter___Server_ChangeDebt_431000436(amount);
L126087 | 			RpcLogic___ChangeDebt_431000436(amount);
L126088 | 		}
L126089 | 
L126090 | 		private void TryRecoverDebt()
L126091 | 		{
L126092 | 			float num = Mathf.Min(SyncAccessor_debt, Stash.CashAmount);
L126093 | 			if (num > 0f)
L126094 | 			{
L126095 | 				UnityEngine.Debug.Log("Recovering debt: " + num);
L126096 | 				float num2 = SyncAccessor_debt;
L126097 | 				Stash.RemoveCash(num);
L126098 | 				ChangeDebt(0f - num);
L126099 | 				RelationData.ChangeRelationship(num / MaxOrderLimit * 0.5f);
L126100 | 				float num3 = num2 - num;
L126101 | 				string text = "I've received " + MoneyManager.FormatAmount(num) + " cash from you.";
L126102 | 				text = ((!(num3 <= 0f)) ? (text + " Your debt is now " + MoneyManager.FormatAmount(num3)) : (text + " Your debt is now paid off."));
L126103 | 				repaymentReminderSent = false;
L126104 | 				base.MSGConversation.SendMessageChain(new MessageChain
L126105 | 				{
L126106 | 					Messages = new List<string> { text },
L126107 | 					id = UnityEngine.Random.Range(int.MinValue, int.MaxValue)
L126108 | 				});
L126109 | 			}
L126110 | 		}
L126111 | 
L126112 | 		private void CompleteDeaddrop()
L126113 | 		{
L126114 | 			Console.Log("Dead drop ready");
L126115 | 			DeadDrop randomEmptyDrop = DeadDrop.GetRandomEmptyDrop(Player.Local.transform.position);
L126116 | 			if (randomEmptyDrop == null)
L126117 | 			{
L126118 | 				Console.LogError("No empty dead drop locations");
L126119 | 				return;
L126120 | 			}
L126121 | 			StringIntPair[] array = deaddropItems;
L126122 | 			foreach (StringIntPair stringIntPair in array)

// <<< END OF BLOCK: LINES 126081 TO 126122





// ################################################################################





// >>> START OF BLOCK: LINES 126142 TO 126182
// ----------------------------------------
L126142 | 			{
L126143 | 				Messages = new List<string> { line },
L126144 | 				id = UnityEngine.Random.Range(int.MinValue, int.MaxValue)
L126145 | 			});
L126146 | 			this.sync___set_value_deadDropPreparing(value: false, asServer: true);
L126147 | 			minsUntilDeaddropReady = -1;
L126148 | 			deaddropItems = null;
L126149 | 			if (onDeaddropReady != null)
L126150 | 			{
L126151 | 				onDeaddropReady.Invoke();
L126152 | 			}
L126153 | 			string guidString = GUIDManager.GenerateUniqueGUID().ToString();
L126154 | 			NetworkSingleton<QuestManager>.Instance.CreateDeaddropCollectionQuest(null, randomEmptyDrop.GUID.ToString(), guidString);
L126155 | 			SetDeaddrop(null, -1);
L126156 | 		}
L126157 | 
L126158 | 		private void SendDebtReminder()
L126159 | 		{
L126160 | 			repaymentReminderSent = true;
L126161 | 			DialogueChain chain = DialogueHandler.Database.GetChain(EDialogueModule.Supplier, "supplier_request_repayment");
L126162 | 			chain.Lines[0] = chain.Lines[0].Replace("<DEBT>", "<color=#46CB4F>" + MoneyManager.FormatAmount(SyncAccessor_debt) + "</color>");
L126163 | 			base.MSGConversation.SendMessageChain(chain.GetMessageChain());
L126164 | 		}
L126165 | 
L126166 | 		protected virtual void MeetupRequested()
L126167 | 		{
L126168 | 			if (InstanceFinder.IsServer)
L126169 | 			{
L126170 | 				int locationIndex;
L126171 | 				SupplierLocation appropriateLocation = GetAppropriateLocation(out locationIndex);
L126172 | 				string line = DialogueHandler.Database.GetLine(EDialogueModule.Generic, "supplier_meet_confirm");
L126173 | 				line = line.Replace("<LOCATION>", appropriateLocation.LocationDescription);
L126174 | 				MessageChain messageChain = new MessageChain();
L126175 | 				messageChain.Messages.Add(line);
L126176 | 				messageChain.id = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
L126177 | 				base.MSGConversation.SendMessageChain(messageChain, 0.5f);
L126178 | 				MeetAtLocation(null, locationIndex, 360);
L126179 | 			}
L126180 | 		}
L126181 | 
L126182 | 		protected virtual void PayDebtRequested()

// <<< END OF BLOCK: LINES 126142 TO 126182





// ################################################################################





// >>> START OF BLOCK: LINES 126849 TO 126889
// ----------------------------------------
L126849 | 		{
L126850 | 			StashPoI.enabled = true;
L126851 | 			IntObj.enabled = true;
L126852 | 		}
L126853 | 
L126854 | 		private void RecalculateCash()
L126855 | 		{
L126856 | 			float num = 0f;
L126857 | 			for (int i = 0; i < Storage.ItemSlots.Count; i++)
L126858 | 			{
L126859 | 				if (Storage.ItemSlots[i] != null && Storage.ItemSlots[i].ItemInstance != null && Storage.ItemSlots[i].ItemInstance is CashInstance)
L126860 | 				{
L126861 | 					num += (Storage.ItemSlots[i].ItemInstance as CashInstance).Balance;
L126862 | 				}
L126863 | 			}
L126864 | 			CashAmount = num;
L126865 | 		}
L126866 | 
L126867 | 		private void Interacted()
L126868 | 		{
L126869 | 			Storage.StorageEntitySubtitle = "You owe " + Supplier.fullName + " <color=#54E717>" + MoneyManager.FormatAmount(Supplier.Debt) + "</color>. Insert cash and exit stash to pay off your debt";
L126870 | 		}
L126871 | 
L126872 | 		public void RemoveCash(float amount)
L126873 | 		{
L126874 | 			float num = amount;
L126875 | 			for (int i = 0; i < Storage.SlotCount; i++)
L126876 | 			{
L126877 | 				if (num <= 0f)
L126878 | 				{
L126879 | 					break;
L126880 | 				}
L126881 | 				if (Storage.ItemSlots[i].ItemInstance != null && Storage.ItemSlots[i].ItemInstance is CashInstance)
L126882 | 				{
L126883 | 					CashInstance cashInstance = Storage.ItemSlots[i].ItemInstance as CashInstance;
L126884 | 					float num2 = Mathf.Min(num, cashInstance.Balance);
L126885 | 					cashInstance.ChangeBalance(0f - num2);
L126886 | 					if (cashInstance.Balance > 0f)
L126887 | 					{
L126888 | 						Storage.ItemSlots[i].SetStoredItem(cashInstance);
L126889 | 					}

// <<< END OF BLOCK: LINES 126849 TO 126889





// ################################################################################





// >>> START OF BLOCK: LINES 128828 TO 128944
// ----------------------------------------
L128828 | 		protected override void Start()
L128829 | 		{
L128830 | 			base.Start();
L128831 | 			questDefeatCartel = NetworkSingleton<QuestManager>.Instance.DefaultQuests.FirstOrDefault((Quest q) => q is Quest_DefeatCartel) as Quest_DefeatCartel;
L128832 | 			if (questDefeatCartel == null)
L128833 | 			{
L128834 | 				UnityEngine.Debug.LogError("Quest_DefeatCartel not found in DefaultQuests.");
L128835 | 			}
L128836 | 		}
L128837 | 
L128838 | 		public override void ChoiceCallback(string choiceLabel)
L128839 | 		{
L128840 | 			WeaponOption weaponOption = allWeapons.Find((WeaponOption x) => x.Name == choiceLabel);
L128841 | 			if (weaponOption != null)
L128842 | 			{
L128843 | 				chosenWeapon = weaponOption;
L128844 | 				handler.ShowNode(DialogueHandler.activeDialogue.GetDialogueNodeByLabel("FINALIZE"));
L128845 | 			}
L128846 | 			if (choiceLabel == "CONFIRM" && chosenWeapon != null)
L128847 | 			{
L128848 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - chosenWeapon.Price);
L128849 | 				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(chosenWeapon.Item.GetDefaultInstance());
L128850 | 			}
L128851 | 			base.ChoiceCallback(choiceLabel);
L128852 | 		}
L128853 | 
L128854 | 		public override void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
L128855 | 		{
L128856 | 			if (dialogueLabel == "MELEE_SELECTION")
L128857 | 			{
L128858 | 				existingChoices.AddRange(GetWeaponChoices(MeleeWeapons));
L128859 | 			}
L128860 | 			if (dialogueLabel == "RANGED_SELECTION")
L128861 | 			{
L128862 | 				existingChoices.AddRange(GetWeaponChoices(RangedWeapons));
L128863 | 			}
L128864 | 			if (dialogueLabel == "AMMO_SELECTION")
L128865 | 			{
L128866 | 				existingChoices.AddRange(GetWeaponChoices(Ammo));
L128867 | 			}
L128868 | 			base.ModifyChoiceList(dialogueLabel, ref existingChoices);
L128869 | 		}
L128870 | 
L128871 | 		private List<DialogueChoiceData> GetWeaponChoices(List<WeaponOption> options)
L128872 | 		{
L128873 | 			List<DialogueChoiceData> list = new List<DialogueChoiceData>();
L128874 | 			foreach (WeaponOption option in options)
L128875 | 			{
L128876 | 				DialogueChoiceData dialogueChoiceData = new DialogueChoiceData();
L128877 | 				dialogueChoiceData.ChoiceText = option.Name + "<color=#54E717> (" + MoneyManager.FormatAmount(option.Price) + ")</color>";
L128878 | 				dialogueChoiceData.ChoiceLabel = option.Name;
L128879 | 				list.Add(dialogueChoiceData);
L128880 | 			}
L128881 | 			return list;
L128882 | 		}
L128883 | 
L128884 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L128885 | 		{
L128886 | 			WeaponOption weaponOption = allWeapons.Find((WeaponOption x) => x.Name == choiceLabel);
L128887 | 			if (weaponOption != null)
L128888 | 			{
L128889 | 				if (!weaponOption.IsAvailable)
L128890 | 				{
L128891 | 					invalidReason = weaponOption.NotAvailableReason;
L128892 | 					return false;
L128893 | 				}
L128894 | 				if (weaponOption.Item.RequiresLevelToPurchase && NetworkSingleton<LevelManager>.Instance.GetFullRank() < weaponOption.Item.RequiredRank)
L128895 | 				{
L128896 | 					FullRank requiredRank = weaponOption.Item.RequiredRank;
L128897 | 					invalidReason = "Available at " + requiredRank.ToString();
L128898 | 					return false;
L128899 | 				}
L128900 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance < weaponOption.Price)
L128901 | 				{
L128902 | 					invalidReason = "Insufficient cash";
L128903 | 					return false;
L128904 | 				}
L128905 | 			}
L128906 | 			if (choiceLabel == "CONFIRM" && !PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(chosenWeapon.Item.GetDefaultInstance()))
L128907 | 			{
L128908 | 				invalidReason = "Inventory full";
L128909 | 				return false;
L128910 | 			}
L128911 | 			if (choiceLabel == "GIVE_RDX" && PlayerSingleton<PlayerInventory>.Instance.GetAmountOfItem(RDX.ID) == 0)
L128912 | 			{
L128913 | 				invalidReason = "No RDX in inventory";
L128914 | 				return false;
L128915 | 			}
L128916 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L128917 | 		}
L128918 | 
L128919 | 		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L128920 | 		{
L128921 | 			if (dialogueLabel == "FINALIZE" && chosenWeapon != null)
L128922 | 			{
L128923 | 				dialogueText = dialogueText.Replace("<WEAPON>", chosenWeapon.Name);
L128924 | 				dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(chosenWeapon.Price) + "</color>");
L128925 | 			}
L128926 | 			if (dialogueLabel == "REQUEST_ACCEPTED")
L128927 | 			{
L128928 | 				questDefeatCartel.EnquireAboutBombEntry.Complete();
L128929 | 			}
L128930 | 			if (dialogueLabel == "BOMB_CREATED")
L128931 | 			{
L128932 | 				TradeRDXForBomb();
L128933 | 			}
L128934 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L128935 | 		}
L128936 | 
L128937 | 		private void TradeRDXForBomb()
L128938 | 		{
L128939 | 			PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem(RDX.ID);
L128940 | 			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(Bomb.GetDefaultInstance());
L128941 | 		}
L128942 | 	}
L128943 | 	public class DialogueController_Billy : DialogueController
L128944 | 	{

// <<< END OF BLOCK: LINES 128828 TO 128944





// ################################################################################





// >>> START OF BLOCK: LINES 129035 TO 129201
// ----------------------------------------
L129035 | 					if (!(ownedProperty.PropertyCode == "rv") && !(ownedProperty.PropertyCode == "motelroom") && !(ownedProperty is Business))
L129036 | 					{
L129037 | 						_ = ownedProperty.GetUnassignedBeds().Count;
L129038 | 						string propertyName = ownedProperty.PropertyName;
L129039 | 						existingChoices.Add(new DialogueChoiceData
L129040 | 						{
L129041 | 							ChoiceText = propertyName,
L129042 | 							ChoiceLabel = ownedProperty.PropertyCode
L129043 | 						});
L129044 | 					}
L129045 | 				}
L129046 | 			}
L129047 | 			base.ModifyChoiceList(dialogueLabel, ref existingChoices);
L129048 | 		}
L129049 | 
L129050 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L129051 | 		{
L129052 | 			if (choiceLabel == "CONFIRM")
L129053 | 			{
L129054 | 				Employee employeePrefab = NetworkSingleton<EmployeeManager>.Instance.GetEmployeePrefab(selectedEmployeeType);
L129055 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance < employeePrefab.SigningFee + Fixer.GetAdditionalSigningFee())
L129056 | 				{
L129057 | 					invalidReason = "Insufficient cash";
L129058 | 					return false;
L129059 | 				}
L129060 | 			}
L129061 | 			foreach (ScheduleOne.Property.Property ownedProperty in ScheduleOne.Property.Property.OwnedProperties)
L129062 | 			{
L129063 | 				if (choiceLabel == ownedProperty.PropertyCode && ownedProperty.Employees.Count >= ownedProperty.EmployeeCapacity)
L129064 | 				{
L129065 | 					invalidReason = "Employee limit reached (" + ownedProperty.Employees.Count + "/" + ownedProperty.EmployeeCapacity + ")";
L129066 | 					return false;
L129067 | 				}
L129068 | 			}
L129069 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L129070 | 		}
L129071 | 
L129072 | 		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L129073 | 		{
L129074 | 			if (dialogueLabel == "FINALIZE")
L129075 | 			{
L129076 | 				Employee employeePrefab = NetworkSingleton<EmployeeManager>.Instance.GetEmployeePrefab(selectedEmployeeType);
L129077 | 				dialogueText = dialogueText.Replace("<SIGN_FEE>", "<color=#54E717>" + MoneyManager.FormatAmount(employeePrefab.SigningFee + Fixer.GetAdditionalSigningFee()) + "</color>");
L129078 | 				dialogueText = dialogueText.Replace("<DAILY_WAGE>", "<color=#54E717>" + MoneyManager.FormatAmount(employeePrefab.DailyWage) + "</color>");
L129079 | 			}
L129080 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L129081 | 		}
L129082 | 
L129083 | 		public override bool DecideBranch(string branchLabel, out int index)
L129084 | 		{
L129085 | 			if (branchLabel == "IS_FIRST_WORKER")
L129086 | 			{
L129087 | 				if (lastConfirmationWasInitial)
L129088 | 				{
L129089 | 					index = 1;
L129090 | 				}
L129091 | 				else
L129092 | 				{
L129093 | 					index = 0;
L129094 | 				}
L129095 | 				return true;
L129096 | 			}
L129097 | 			return base.DecideBranch(branchLabel, out index);
L129098 | 		}
L129099 | 
L129100 | 		private void Confirm()
L129101 | 		{
L129102 | 			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ClipboardAcquired"))
L129103 | 			{
L129104 | 				lastConfirmationWasInitial = true;
L129105 | 				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ClipboardAcquired", true.ToString());
L129106 | 			}
L129107 | 			else
L129108 | 			{
L129109 | 				lastConfirmationWasInitial = false;
L129110 | 			}
L129111 | 			Employee employeePrefab = NetworkSingleton<EmployeeManager>.Instance.GetEmployeePrefab(selectedEmployeeType);
L129112 | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - (employeePrefab.SigningFee + Fixer.GetAdditionalSigningFee()));
L129113 | 			NetworkSingleton<EmployeeManager>.Instance.CreateNewEmployee(selectedProperty, selectedEmployeeType);
L129114 | 		}
L129115 | 	}
L129116 | 	public class DialogueController_Jen : DialogueController
L129117 | 	{
L129118 | 		public string BuyKeyText = "I want to buy a sewer access key";
L129119 | 
L129120 | 		public StorableItemDefinition KeyItem;
L129121 | 
L129122 | 		public DialogueContainer BuyKeyDialogue;
L129123 | 
L129124 | 		public float MinRelationToBuyKey = 3f;
L129125 | 
L129126 | 		protected override void Start()
L129127 | 		{
L129128 | 			base.Start();
L129129 | 			DialogueChoice dialogueChoice = new DialogueChoice();
L129130 | 			dialogueChoice.ChoiceText = BuyKeyText;
L129131 | 			dialogueChoice.Conversation = BuyKeyDialogue;
L129132 | 			dialogueChoice.Enabled = true;
L129133 | 			dialogueChoice.isValidCheck = CanBuyKey;
L129134 | 			AddDialogueChoice(dialogueChoice);
L129135 | 		}
L129136 | 
L129137 | 		private bool CanBuyKey(out string invalidReason)
L129138 | 		{
L129139 | 			invalidReason = string.Empty;
L129140 | 			if (npc.RelationData.RelationDelta < MinRelationToBuyKey)
L129141 | 			{
L129142 | 				invalidReason = "'" + RelationshipCategory.GetCategory(MinRelationToBuyKey).ToString() + "' relationship required";
L129143 | 				return false;
L129144 | 			}
L129145 | 			return true;
L129146 | 		}
L129147 | 
L129148 | 		public override string ModifyChoiceText(string choiceLabel, string choiceText)
L129149 | 		{
L129150 | 			if (choiceLabel == "CHOICE_CONFIRM")
L129151 | 			{
L129152 | 				choiceText = choiceText.Replace("<PRICE>", "<color=#54E717>(" + MoneyManager.FormatAmount(KeyItem.BasePurchasePrice) + ")</color>");
L129153 | 			}
L129154 | 			return base.ModifyChoiceText(choiceLabel, choiceText);
L129155 | 		}
L129156 | 
L129157 | 		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L129158 | 		{
L129159 | 			if (dialogueLabel == "OFFER")
L129160 | 			{
L129161 | 				dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(KeyItem.BasePurchasePrice) + "</color>");
L129162 | 			}
L129163 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L129164 | 		}
L129165 | 
L129166 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L129167 | 		{
L129168 | 			if (choiceLabel == "CHOICE_CONFIRM" && NetworkSingleton<MoneyManager>.Instance.cashBalance < KeyItem.BasePurchasePrice)
L129169 | 			{
L129170 | 				invalidReason = "Insufficient cash";
L129171 | 				return false;
L129172 | 			}
L129173 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L129174 | 		}
L129175 | 
L129176 | 		public override void ChoiceCallback(string choiceLabel)
L129177 | 		{
L129178 | 			if (choiceLabel == "CHOICE_CONFIRM")
L129179 | 			{
L129180 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - KeyItem.BasePurchasePrice, visualizeChange: true, playCashSound: true);
L129181 | 				npc.Inventory.InsertItem(NetworkSingleton<MoneyManager>.Instance.GetCashInstance(KeyItem.BasePurchasePrice));
L129182 | 				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(KeyItem.GetDefaultInstance());
L129183 | 			}
L129184 | 			base.ChoiceCallback(choiceLabel);
L129185 | 		}
L129186 | 	}
L129187 | 	public class DialogueController_Ming : DialogueController
L129188 | 	{
L129189 | 		public ScheduleOne.Property.Property Property;
L129190 | 
L129191 | 		public float Price = 500f;
L129192 | 
L129193 | 		public DialogueContainer BuyDialogue;
L129194 | 
L129195 | 		public string BuyText = "I'd like to buy the room";
L129196 | 
L129197 | 		public string RemindText = "Where is my room?";
L129198 | 
L129199 | 		public DialogueContainer RemindLocationDialogue;
L129200 | 
L129201 | 		public QuestEntry[] PurchaseRoomQuests;

// <<< END OF BLOCK: LINES 129035 TO 129201





// ################################################################################





// >>> START OF BLOCK: LINES 129219 TO 129443
// ----------------------------------------
L129219 | 			AddDialogueChoice(dialogueChoice2);
L129220 | 		}
L129221 | 
L129222 | 		private bool CanBuyRoom(bool enabled)
L129223 | 		{
L129224 | 			if (!Property.IsOwned)
L129225 | 			{
L129226 | 				if (PurchaseRoomQuests.Length != 0)
L129227 | 				{
L129228 | 					return PurchaseRoomQuests.FirstOrDefault((QuestEntry q) => q.State == EQuestState.Active) != null;
L129229 | 				}
L129230 | 				return true;
L129231 | 			}
L129232 | 			return false;
L129233 | 		}
L129234 | 
L129235 | 		public override string ModifyChoiceText(string choiceLabel, string choiceText)
L129236 | 		{
L129237 | 			if (choiceLabel == "CHOICE_CONFIRM")
L129238 | 			{
L129239 | 				choiceText = choiceText.Replace("<PRICE>", "<color=#54E717>(" + MoneyManager.FormatAmount(Price) + ")</color>");
L129240 | 			}
L129241 | 			return base.ModifyChoiceText(choiceLabel, choiceText);
L129242 | 		}
L129243 | 
L129244 | 		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L129245 | 		{
L129246 | 			if (dialogueLabel == "ENTRY")
L129247 | 			{
L129248 | 				dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(Price) + "</color>");
L129249 | 			}
L129250 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L129251 | 		}
L129252 | 
L129253 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L129254 | 		{
L129255 | 			if (choiceLabel == "CHOICE_CONFIRM" && NetworkSingleton<MoneyManager>.Instance.cashBalance < Price)
L129256 | 			{
L129257 | 				invalidReason = "Insufficient cash";
L129258 | 				return false;
L129259 | 			}
L129260 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L129261 | 		}
L129262 | 
L129263 | 		public override void ChoiceCallback(string choiceLabel)
L129264 | 		{
L129265 | 			if (choiceLabel == "CHOICE_CONFIRM")
L129266 | 			{
L129267 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - Price);
L129268 | 				npc.Inventory.InsertItem(NetworkSingleton<MoneyManager>.Instance.GetCashInstance(Price));
L129269 | 				Property.SetOwned();
L129270 | 				if (onPurchase != null)
L129271 | 				{
L129272 | 					onPurchase.Invoke();
L129273 | 				}
L129274 | 			}
L129275 | 			base.ChoiceCallback(choiceLabel);
L129276 | 		}
L129277 | 	}
L129278 | 	public class DialogueController_Sam : DialogueController
L129279 | 	{
L129280 | 		private Quest_DefeatCartel questDefeatCartel;
L129281 | 
L129282 | 		protected override void Start()
L129283 | 		{
L129284 | 			base.Start();
L129285 | 			questDefeatCartel = NetworkSingleton<QuestManager>.Instance.DefaultQuests.FirstOrDefault((Quest q) => q is Quest_DefeatCartel) as Quest_DefeatCartel;
L129286 | 			if (questDefeatCartel == null)
L129287 | 			{
L129288 | 				UnityEngine.Debug.LogError("Quest_DefeatCartel not found in DefaultQuests.");
L129289 | 			}
L129290 | 		}
L129291 | 
L129292 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L129293 | 		{
L129294 | 			if (choiceLabel == "CONFIRM_DIG" && NetworkSingleton<MoneyManager>.Instance.cashBalance <= 10000f)
L129295 | 			{
L129296 | 				invalidReason = "Insufficient cash";
L129297 | 				return false;
L129298 | 			}
L129299 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L129300 | 		}
L129301 | 
L129302 | 		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L129303 | 		{
L129304 | 			if (dialogueLabel == "DIG_OFFER")
L129305 | 			{
L129306 | 				return dialogueText.Replace("<DIG_PRICE>", MoneyManager.ApplyMoneyTextColor(MoneyManager.FormatAmount(10000f)));
L129307 | 			}
L129308 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L129309 | 		}
L129310 | 
L129311 | 		public override void ChoiceCallback(string choiceLabel)
L129312 | 		{
L129313 | 			if (choiceLabel == "CONFIRM_DIG")
L129314 | 			{
L129315 | 				questDefeatCartel.DigTunnelEntry.Complete();
L129316 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-10000f, visualizeChange: true, playCashSound: true);
L129317 | 			}
L129318 | 			base.ChoiceCallback(choiceLabel);
L129319 | 		}
L129320 | 	}
L129321 | 	public class DialogueController_SkateboardSeller : DialogueController
L129322 | 	{
L129323 | 		[Serializable]
L129324 | 		public class Option
L129325 | 		{
L129326 | 			public string Name;
L129327 | 
L129328 | 			public float Price;
L129329 | 
L129330 | 			public bool IsAvailable;
L129331 | 
L129332 | 			public string NotAvailableReason;
L129333 | 
L129334 | 			public ItemDefinition Item;
L129335 | 		}
L129336 | 
L129337 | 		public List<Option> Options = new List<Option>();
L129338 | 
L129339 | 		private Option chosenWeapon;
L129340 | 
L129341 | 		public UnityEvent onPurchase;
L129342 | 
L129343 | 		private void Awake()
L129344 | 		{
L129345 | 		}
L129346 | 
L129347 | 		public override void ChoiceCallback(string choiceLabel)
L129348 | 		{
L129349 | 			Option option = Options.Find((Option x) => x.Name == choiceLabel);
L129350 | 			if (option != null)
L129351 | 			{
L129352 | 				chosenWeapon = option;
L129353 | 				handler.ShowNode(DialogueHandler.activeDialogue.GetDialogueNodeByLabel("FINALIZE"));
L129354 | 			}
L129355 | 			if (choiceLabel == "CONFIRM" && chosenWeapon != null)
L129356 | 			{
L129357 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - chosenWeapon.Price);
L129358 | 				PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(chosenWeapon.Item.GetDefaultInstance());
L129359 | 				npc.Inventory.InsertItem(NetworkSingleton<MoneyManager>.Instance.GetCashInstance(chosenWeapon.Price));
L129360 | 				if (chosenWeapon.Item.ID == "goldenskateboard")
L129361 | 				{
L129362 | 					Singleton<AchievementManager>.Instance.UnlockAchievement(AchievementManager.EAchievement.ROLLING_IN_STYLE);
L129363 | 				}
L129364 | 				if (onPurchase != null)
L129365 | 				{
L129366 | 					onPurchase.Invoke();
L129367 | 				}
L129368 | 			}
L129369 | 			base.ChoiceCallback(choiceLabel);
L129370 | 		}
L129371 | 
L129372 | 		public override void ModifyChoiceList(string dialogueLabel, ref List<DialogueChoiceData> existingChoices)
L129373 | 		{
L129374 | 			if (dialogueLabel == "ENTRY" && DialogueHandler.activeDialogue.name == "Skateboard_Sell")
L129375 | 			{
L129376 | 				existingChoices.AddRange(GetChoices(Options));
L129377 | 			}
L129378 | 			base.ModifyChoiceList(dialogueLabel, ref existingChoices);
L129379 | 		}
L129380 | 
L129381 | 		private List<DialogueChoiceData> GetChoices(List<Option> options)
L129382 | 		{
L129383 | 			List<DialogueChoiceData> list = new List<DialogueChoiceData>();
L129384 | 			foreach (Option option in options)
L129385 | 			{
L129386 | 				DialogueChoiceData dialogueChoiceData = new DialogueChoiceData();
L129387 | 				dialogueChoiceData.ChoiceText = option.Name + "<color=#54E717> (" + MoneyManager.FormatAmount(option.Price) + ")</color>";
L129388 | 				dialogueChoiceData.ChoiceLabel = option.Name;
L129389 | 				list.Add(dialogueChoiceData);
L129390 | 			}
L129391 | 			return list;
L129392 | 		}
L129393 | 
L129394 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L129395 | 		{
L129396 | 			Option option = Options.Find((Option x) => x.Name == choiceLabel);
L129397 | 			if (option != null)
L129398 | 			{
L129399 | 				if (!option.IsAvailable)
L129400 | 				{
L129401 | 					invalidReason = option.NotAvailableReason;
L129402 | 					return false;
L129403 | 				}
L129404 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance < option.Price)
L129405 | 				{
L129406 | 					invalidReason = "Insufficient cash";
L129407 | 					return false;
L129408 | 				}
L129409 | 			}
L129410 | 			if (choiceLabel == "CONFIRM" && !PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(chosenWeapon.Item.GetDefaultInstance()))
L129411 | 			{
L129412 | 				invalidReason = "Inventory full";
L129413 | 				return false;
L129414 | 			}
L129415 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L129416 | 		}
L129417 | 
L129418 | 		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L129419 | 		{
L129420 | 			if (dialogueLabel == "FINALIZE" && chosenWeapon != null)
L129421 | 			{
L129422 | 				dialogueText = dialogueText.Replace("<NAME>", chosenWeapon.Name);
L129423 | 				dialogueText = dialogueText.Replace("<PRICE>", "<color=#54E717>" + MoneyManager.FormatAmount(chosenWeapon.Price) + "</color>");
L129424 | 			}
L129425 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L129426 | 		}
L129427 | 	}
L129428 | 	public class DialogueController_ThomasBenzies : DialogueController
L129429 | 	{
L129430 | 		public override void ChoiceCallback(string choiceLabel)
L129431 | 		{
L129432 | 			base.ChoiceCallback(choiceLabel);
L129433 | 			if (choiceLabel == "ACCEPT_DEAL")
L129434 | 			{
L129435 | 				NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.SetStatus_Server(ECartelStatus.Truced, resetStatusChangedTimer: true);
L129436 | 				NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.DealManager.SetHoursUntilDealRequest(12);
L129437 | 				WaitForConversationEnd();
L129438 | 			}
L129439 | 			else if (choiceLabel == "REFUSE_DEAL")
L129440 | 			{
L129441 | 				NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.SetStatus_Server(ECartelStatus.Hostile, resetStatusChangedTimer: true);
L129442 | 				WaitForConversationEnd();
L129443 | 			}

// <<< END OF BLOCK: LINES 129219 TO 129443





// ################################################################################





// >>> START OF BLOCK: LINES 129903 TO 129972
// ----------------------------------------
L129903 | 
L129904 | 		public void SetDialogueEnabled(bool enabled)
L129905 | 		{
L129906 | 			DialogueEnabled = enabled;
L129907 | 		}
L129908 | 	}
L129909 | 	public class DialogueController_Dealer : DialogueController
L129910 | 	{
L129911 | 		public Dealer Dealer { get; private set; }
L129912 | 
L129913 | 		protected override void Start()
L129914 | 		{
L129915 | 			base.Start();
L129916 | 			Dealer = npc as Dealer;
L129917 | 		}
L129918 | 
L129919 | 		public override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L129920 | 		{
L129921 | 			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && dialogueLabel == "ENTRY")
L129922 | 			{
L129923 | 				dialogueText = dialogueText.Replace("<SIGNING_FEE>", "<color=#54E717>" + MoneyManager.FormatAmount(Dealer.SigningFee) + "</color>");
L129924 | 				dialogueText = dialogueText.Replace("<CUT>", "<color=#54E717>" + Mathf.RoundToInt(Dealer.Cut * 100f) + "%</color>");
L129925 | 			}
L129926 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L129927 | 		}
L129928 | 
L129929 | 		public override string ModifyChoiceText(string choiceLabel, string choiceText)
L129930 | 		{
L129931 | 			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && choiceLabel == "CONFIRM")
L129932 | 			{
L129933 | 				choiceText = choiceText.Replace("<SIGNING_FEE>", "<color=#54E717>" + MoneyManager.FormatAmount(Dealer.SigningFee) + "</color>");
L129934 | 			}
L129935 | 			return base.ModifyChoiceText(choiceLabel, choiceText);
L129936 | 		}
L129937 | 
L129938 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L129939 | 		{
L129940 | 			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && choiceLabel == "CONFIRM" && NetworkSingleton<MoneyManager>.Instance.cashBalance < Dealer.SigningFee)
L129941 | 			{
L129942 | 				invalidReason = "Insufficient cash";
L129943 | 				return false;
L129944 | 			}
L129945 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L129946 | 		}
L129947 | 
L129948 | 		public override void ChoiceCallback(string choiceLabel)
L129949 | 		{
L129950 | 			if (DialogueHandler.activeDialogue.name == "Supplier_Recruitment" && choiceLabel == "CONFIRM")
L129951 | 			{
L129952 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - Dealer.SigningFee);
L129953 | 				Dealer.InitialRecruitment();
L129954 | 			}
L129955 | 			base.ChoiceCallback(choiceLabel);
L129956 | 		}
L129957 | 	}
L129958 | 	public class DialogueController_Employee : DialogueController
L129959 | 	{
L129960 | 		private ScheduleOne.Property.Property selectedProperty;
L129961 | 
L129962 | 		private void Awake()
L129963 | 		{
L129964 | 		}
L129965 | 
L129966 | 		public override void ChoiceCallback(string choiceLabel)
L129967 | 		{
L129968 | 			ScheduleOne.Property.Property propertyByCode = GetPropertyByCode(choiceLabel);
L129969 | 			if (propertyByCode != null)
L129970 | 			{
L129971 | 				selectedProperty = propertyByCode;
L129972 | 				handler.ShowNode(DialogueHandler.activeDialogue.GetDialogueNodeByLabel("FINALIZE"));

// <<< END OF BLOCK: LINES 129903 TO 129972





// ################################################################################





// >>> START OF BLOCK: LINES 130733 TO 130864
// ----------------------------------------
L130733 | 		}
L130734 | 
L130735 | 		public virtual void ShowWorldspaceDialogue_5s(string text)
L130736 | 		{
L130737 | 			ShowWorldspaceDialogue(text, 5f);
L130738 | 		}
L130739 | 	}
L130740 | 	public class DialogueHandler_Customer : DialogueHandler
L130741 | 	{
L130742 | 	}
L130743 | 	public class DialogueHandler_EstateAgent : ControlledDialogueHandler
L130744 | 	{
L130745 | 		private ScheduleOne.Property.Property selectedProperty;
L130746 | 
L130747 | 		private Business selectedBusiness;
L130748 | 
L130749 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L130750 | 		{
L130751 | 			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130752 | 			Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130753 | 			if (property != null && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < property.Price)
L130754 | 			{
L130755 | 				invalidReason = "Insufficient balance";
L130756 | 				return false;
L130757 | 			}
L130758 | 			if (business != null && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < business.Price)
L130759 | 			{
L130760 | 				invalidReason = "Insufficient balance";
L130761 | 				return false;
L130762 | 			}
L130763 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L130764 | 		}
L130765 | 
L130766 | 		public override bool ShouldChoiceBeShown(string choiceLabel)
L130767 | 		{
L130768 | 			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.Properties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130769 | 			Business business = Business.Businesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130770 | 			if (property != null && (property.IsOwned || !property.CanBePurchased()))
L130771 | 			{
L130772 | 				return false;
L130773 | 			}
L130774 | 			if (business != null && (business.IsOwned || !business.CanBePurchased()))
L130775 | 			{
L130776 | 				return false;
L130777 | 			}
L130778 | 			return base.ShouldChoiceBeShown(choiceLabel);
L130779 | 		}
L130780 | 
L130781 | 		protected override void ChoiceCallback(string choiceLabel)
L130782 | 		{
L130783 | 			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130784 | 			Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130785 | 			if (property != null)
L130786 | 			{
L130787 | 				selectedProperty = property;
L130788 | 			}
L130789 | 			if (business != null)
L130790 | 			{
L130791 | 				selectedBusiness = business;
L130792 | 			}
L130793 | 			base.ChoiceCallback(choiceLabel);
L130794 | 		}
L130795 | 
L130796 | 		protected override void DialogueCallback(string choiceLabel)
L130797 | 		{
L130798 | 			if (choiceLabel == "CONFIRM_BUY")
L130799 | 			{
L130800 | 				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(selectedProperty.PropertyName + " purchase", 0f - selectedProperty.Price, 1f, string.Empty);
L130801 | 				selectedProperty.SetOwned();
L130802 | 			}
L130803 | 			if (choiceLabel == "CONFIRM_BUY_BUSINESS")
L130804 | 			{
L130805 | 				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(selectedBusiness.PropertyName + " purchase", 0f - selectedBusiness.Price, 1f, string.Empty);
L130806 | 				selectedBusiness.SetOwned();
L130807 | 			}
L130808 | 			base.DialogueCallback(choiceLabel);
L130809 | 		}
L130810 | 
L130811 | 		protected override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L130812 | 		{
L130813 | 			if (dialogueLabel == "CONFIRM")
L130814 | 			{
L130815 | 				return dialogueText.Replace("<PROPERTY>", selectedProperty.PropertyName.ToLower());
L130816 | 			}
L130817 | 			if (dialogueLabel == "CONFIRM_BUSINESS")
L130818 | 			{
L130819 | 				return dialogueText.Replace("<BUSINESS>", selectedBusiness.PropertyName.ToLower());
L130820 | 			}
L130821 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);
L130822 | 		}
L130823 | 
L130824 | 		protected override string ModifyChoiceText(string choiceLabel, string choiceText)
L130825 | 		{
L130826 | 			ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130827 | 			Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == choiceLabel.ToLower());
L130828 | 			if (property != null)
L130829 | 			{
L130830 | 				return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(property.Price) + ")</color>");
L130831 | 			}
L130832 | 			if (business != null)
L130833 | 			{
L130834 | 				return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(business.Price) + ")</color>");
L130835 | 			}
L130836 | 			if (choiceLabel == "CONFIRM_CHOICE")
L130837 | 			{
L130838 | 				if (selectedProperty != null)
L130839 | 				{
L130840 | 					return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedProperty.Price) + ")</color>");
L130841 | 				}
L130842 | 				if (selectedBusiness != null)
L130843 | 				{
L130844 | 					return choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedBusiness.Price) + ")</color>");
L130845 | 				}
L130846 | 			}
L130847 | 			return base.ModifyChoiceText(choiceLabel, choiceText);
L130848 | 		}
L130849 | 	}
L130850 | 	public class DialogueHandler_Police : ControlledDialogueHandler
L130851 | 	{
L130852 | 		[Header("References")]
L130853 | 		public DialogueContainer CheckpointRequestDialogue;
L130854 | 
L130855 | 		private PoliceOfficer officer;
L130856 | 
L130857 | 		protected override void Awake()
L130858 | 		{
L130859 | 			base.Awake();
L130860 | 			officer = base.NPC as PoliceOfficer;
L130861 | 		}
L130862 | 
L130863 | 		public override void Hovered()
L130864 | 		{

// <<< END OF BLOCK: LINES 130733 TO 130864





// ################################################################################





// >>> START OF BLOCK: LINES 130896 TO 131009
// ----------------------------------------
L130896 | 					return 1;
L130897 | 				}
L130898 | 				return 0;
L130899 | 			}
L130900 | 			return base.CheckBranch(branchLabel);
L130901 | 		}
L130902 | 	}
L130903 | 	public class DialogueHandler_VehicleSalesman : ControlledDialogueHandler
L130904 | 	{
L130905 | 		public Jeremy Salesman;
L130906 | 
L130907 | 		public Jeremy.DealershipListing selectedVehicle;
L130908 | 
L130909 | 		protected override string ModifyChoiceText(string choiceLabel, string choiceText)
L130910 | 		{
L130911 | 			Jeremy.DealershipListing dealershipListing = Salesman.Listings.Find((Jeremy.DealershipListing x) => x.vehicleCode.ToLower() == choiceLabel.ToLower());
L130912 | 			if (choiceLabel == "BUY_CASH")
L130913 | 			{
L130914 | 				if (selectedVehicle != null)
L130915 | 				{
L130916 | 					choiceText = choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedVehicle.price) + ")</color>");
L130917 | 				}
L130918 | 			}
L130919 | 			else if (choiceLabel == "BUY_ONLINE")
L130920 | 			{
L130921 | 				if (selectedVehicle != null)
L130922 | 				{
L130923 | 					choiceText = choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(selectedVehicle.price) + ")</color>");
L130924 | 				}
L130925 | 			}
L130926 | 			else if (dealershipListing != null)
L130927 | 			{
L130928 | 				choiceText = choiceText.Replace("(<PRICE>)", "<color=#19BEF0>(" + MoneyManager.FormatAmount(dealershipListing.price) + ")</color>");
L130929 | 			}
L130930 | 			return base.ModifyChoiceText(choiceLabel, choiceText);
L130931 | 		}
L130932 | 
L130933 | 		public override bool CheckChoice(string choiceLabel, out string invalidReason)
L130934 | 		{
L130935 | 			if (choiceLabel == "BUY_CASH")
L130936 | 			{
L130937 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance < selectedVehicle.price)
L130938 | 				{
L130939 | 					invalidReason = "Insufficient cash";
L130940 | 					return false;
L130941 | 				}
L130942 | 			}
L130943 | 			else if (choiceLabel == "BUY_ONLINE" && NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance < selectedVehicle.price)
L130944 | 			{
L130945 | 				invalidReason = "Insufficient balance";
L130946 | 				return false;
L130947 | 			}
L130948 | 			return base.CheckChoice(choiceLabel, out invalidReason);
L130949 | 		}
L130950 | 
L130951 | 		protected override void ChoiceCallback(string choiceLabel)
L130952 | 		{
L130953 | 			if (choiceLabel == "BUY_CASH")
L130954 | 			{
L130955 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= selectedVehicle.price)
L130956 | 				{
L130957 | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - selectedVehicle.price);
L130958 | 					NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
L130959 | 					Salesman.Dealership.SpawnVehicle(selectedVehicle.vehicleCode);
L130960 | 				}
L130961 | 				return;
L130962 | 			}
L130963 | 			if (choiceLabel == "BUY_ONLINE")
L130964 | 			{
L130965 | 				if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= selectedVehicle.price)
L130966 | 				{
L130967 | 					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction(selectedVehicle.vehicleCode + " purchase", 0f - selectedVehicle.price, 1f, string.Empty);
L130968 | 					NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
L130969 | 					Salesman.Dealership.SpawnVehicle(selectedVehicle.vehicleCode);
L130970 | 				}
L130971 | 				return;
L130972 | 			}
L130973 | 			Jeremy.DealershipListing dealershipListing = Salesman.Listings.Find((Jeremy.DealershipListing x) => x.vehicleCode.ToLower() == choiceLabel.ToLower());
L130974 | 			if (dealershipListing != null)
L130975 | 			{
L130976 | 				selectedVehicle = dealershipListing;
L130977 | 			}
L130978 | 			base.ChoiceCallback(choiceLabel);
L130979 | 		}
L130980 | 
L130981 | 		protected override int CheckBranch(string branchLabel)
L130982 | 		{
L130983 | 			if (branchLabel == "BRANCH_CAN_AFFORD")
L130984 | 			{
L130985 | 				if (selectedVehicle == null)
L130986 | 				{
L130987 | 					return 0;
L130988 | 				}
L130989 | 				if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= selectedVehicle.price)
L130990 | 				{
L130991 | 					return 1;
L130992 | 				}
L130993 | 				return 0;
L130994 | 			}
L130995 | 			return base.CheckBranch(branchLabel);
L130996 | 		}
L130997 | 
L130998 | 		protected override void DialogueCallback(string choiceLabel)
L130999 | 		{
L131000 | 			base.DialogueCallback(choiceLabel);
L131001 | 		}
L131002 | 
L131003 | 		protected override string ModifyDialogueText(string dialogueLabel, string dialogueText)
L131004 | 		{
L131005 | 			if (dialogueLabel == "CONFIRM")
L131006 | 			{
L131007 | 				return dialogueText.Replace("<VEHICLE>", selectedVehicle.vehicleName);
L131008 | 			}
L131009 | 			return base.ModifyDialogueText(dialogueLabel, dialogueText);

// <<< END OF BLOCK: LINES 130896 TO 131009





// ################################################################################





// >>> START OF BLOCK: LINES 135379 TO 135419
// ----------------------------------------
L135379 | 					else
L135380 | 					{
L135381 | 						ItemInstance defaultInstance = randomInventoryItem.GetDefaultInstance();
L135382 | 						InsertItem(defaultInstance);
L135383 | 					}
L135384 | 					if (!AllowDuplicateRandomItems)
L135385 | 					{
L135386 | 						list.Add(randomInventoryItem.ID);
L135387 | 					}
L135388 | 					continue;
L135389 | 				}
L135390 | 				break;
L135391 | 			}
L135392 | 		}
L135393 | 
L135394 | 		private void AddRandomCashInstance()
L135395 | 		{
L135396 | 			int num = UnityEngine.Random.Range(RandomCashMin, RandomCashMax);
L135397 | 			if (num > 0)
L135398 | 			{
L135399 | 				CashInstance cashInstance = NetworkSingleton<MoneyManager>.Instance.GetCashInstance(num);
L135400 | 				InsertItem(cashInstance);
L135401 | 			}
L135402 | 		}
L135403 | 
L135404 | 		private StorableItemDefinition GetRandomInventoryItem(List<string> excludeIDs)
L135405 | 		{
L135406 | 			float num = UnityEngine.Random.Range(0f, GetTotalRandomInventoryItemWeight());
L135407 | 			float num2 = 0f;
L135408 | 			for (int i = 0; i < RandomInventoryItems.Length; i++)
L135409 | 			{
L135410 | 				num2 += RandomInventoryItems[i].Weight;
L135411 | 				if (num <= num2 && !excludeIDs.Contains(RandomInventoryItems[i].ItemDefinition.ID))
L135412 | 				{
L135413 | 					return RandomInventoryItems[i].ItemDefinition;
L135414 | 				}
L135415 | 			}
L135416 | 			return null;
L135417 | 		}
L135418 | 
L135419 | 		[Button]

// <<< END OF BLOCK: LINES 135379 TO 135419





// ################################################################################





// >>> START OF BLOCK: LINES 135685 TO 135725
// ----------------------------------------
L135685 | 				{
L135686 | 					break;
L135687 | 				}
L135688 | 				if (ItemSlots[i].ItemInstance is CashInstance cashInstance)
L135689 | 				{
L135690 | 					float num = Mathf.Min(amountToRemove, cashInstance.Balance);
L135691 | 					cashInstance.ChangeBalance(0f - num);
L135692 | 					amountToRemove -= num;
L135693 | 				}
L135694 | 			}
L135695 | 		}
L135696 | 
L135697 | 		public void AddCash(float amountToAdd)
L135698 | 		{
L135699 | 			if (!(amountToAdd <= 0f))
L135700 | 			{
L135701 | 				while (amountToAdd > 0.1f)
L135702 | 				{
L135703 | 					float num = Mathf.Min(amountToAdd, 1000f);
L135704 | 					amountToAdd -= num;
L135705 | 					CashInstance cashInstance = NetworkSingleton<MoneyManager>.Instance.GetCashInstance(num);
L135706 | 					InsertItem(cashInstance);
L135707 | 				}
L135708 | 			}
L135709 | 		}
L135710 | 
L135711 | 		[ServerRpc(RunLocally = true, RequireOwnership = false)]
L135712 | 		public void SetStoredInstance(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
L135713 | 		{
L135714 | 			RpcWriter___Server_SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
L135715 | 			RpcLogic___SetStoredInstance_2652194801(conn, itemSlotIndex, instance);
L135716 | 		}
L135717 | 
L135718 | 		[ObserversRpc(RunLocally = true)]
L135719 | 		[TargetRpc]
L135720 | 		private void SetStoredInstance_Internal(NetworkConnection conn, int itemSlotIndex, ItemInstance instance)
L135721 | 		{
L135722 | 			if ((object)conn == null)
L135723 | 			{
L135724 | 				RpcWriter___Observers_SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);
L135725 | 				RpcLogic___SetStoredInstance_Internal_2652194801(conn, itemSlotIndex, instance);

// <<< END OF BLOCK: LINES 135685 TO 135725





// ################################################################################





// >>> START OF BLOCK: LINES 167365 TO 167405
// ----------------------------------------
L167365 | 			player3Hand.Clear();
L167366 | 			player4Hand.Clear();
L167367 | 			dealerHand.Clear();
L167368 | 			drawnCardsValues.Clear();
L167369 | 		}
L167370 | 
L167371 | 		[ObserversRpc(RunLocally = true)]
L167372 | 		private void EndGame()
L167373 | 		{
L167374 | 			RpcWriter___Observers_EndGame_2166136261();
L167375 | 			RpcLogic___EndGame_2166136261();
L167376 | 		}
L167377 | 
L167378 | 		public void RemoveLocalPlayerFromGame(EPayoutType payout, float cameraDelay = 0f)
L167379 | 		{
L167380 | 			RequestRemovePlayerFromCurrentRound(Player.Local.NetworkObject);
L167381 | 			Players.SetPlayerScore(Player.Local, 0);
L167382 | 			float payout2 = GetPayout(LocalPlayerBet, payout);
L167383 | 			if (payout2 > 0f)
L167384 | 			{
L167385 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(payout2);
L167386 | 			}
L167387 | 			if (onLocalPlayerRoundCompleted != null)
L167388 | 			{
L167389 | 				onLocalPlayerRoundCompleted(payout);
L167390 | 			}
L167391 | 			if (onLocalPlayerExitRound != null)
L167392 | 			{
L167393 | 				onLocalPlayerExitRound();
L167394 | 			}
L167395 | 			StartCoroutine(Wait());
L167396 | 			IEnumerator Wait()
L167397 | 			{
L167398 | 				yield return new WaitForSeconds(cameraDelay);
L167399 | 				if (base.IsOpen && !IsLocalPlayerInCurrentRound)
L167400 | 				{
L167401 | 					PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(localDefaultCameraTransform.position, localDefaultCameraTransform.rotation, 0.5f);
L167402 | 				}
L167403 | 			}
L167404 | 		}
L167405 | 

// <<< END OF BLOCK: LINES 167365 TO 167405





// ################################################################################





// >>> START OF BLOCK: LINES 167537 TO 167577
// ----------------------------------------
L167537 | 				Channel channel = Channel.Reliable;
L167538 | 				PooledWriter writer = WriterPool.GetWriter();
L167539 | 				SendObserversRpc(0u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
L167540 | 				writer.Store();
L167541 | 			}
L167542 | 		}
L167543 | 
L167544 | 		private void RpcLogic___StartGame_2166136261()
L167545 | 		{
L167546 | 			ResetCards();
L167547 | 			CurrentStage = EStage.Dealing;
L167548 | 			PlayerTurn = null;
L167549 | 			IsLocalPlayerBlackjack = false;
L167550 | 			IsLocalPlayerBust = false;
L167551 | 			if (InstanceFinder.IsServer)
L167552 | 			{
L167553 | 				SetRoundEnded(ended: false);
L167554 | 			}
L167555 | 			if (IsLocalPlayerInCurrentRound)
L167556 | 			{
L167557 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - LocalPlayerBet);
L167558 | 				Players.SetPlayerScore(Player.Local, Mathf.RoundToInt(LocalPlayerBet));
L167559 | 				base.LocalPlayerData.SetData("Ready", value: false);
L167560 | 			}
L167561 | 			List<Player> clockwisePlayers = GetClockwisePlayers();
L167562 | 			if (gameRoutine != null)
L167563 | 			{
L167564 | 				Console.LogWarning("Game routine already running, stopping...");
L167565 | 				StopCoroutine(gameRoutine);
L167566 | 			}
L167567 | 			gameRoutine = Singleton<CoroutineService>.Instance.StartCoroutine(GameRoutine());
L167568 | 			IEnumerator GameRoutine()
L167569 | 			{
L167570 | 				float drawSpacing = Mathf.Lerp(0.5f, 0.2f, (float)clockwisePlayers.Count / 4f);
L167571 | 				Console.Log("Dealing...");
L167572 | 				for (int i = 0; i < clockwisePlayers.Count; i++)
L167573 | 				{
L167574 | 					if (playersInCurrentRound.Contains(clockwisePlayers[i]))
L167575 | 					{
L167576 | 						int playerIndex = Players.GetPlayerIndex(clockwisePlayers[i]);
L167577 | 						Transform[] playerCardPositions = GetPlayerCardPositions(playerIndex);

// <<< END OF BLOCK: LINES 167537 TO 167577





// ################################################################################





// >>> START OF BLOCK: LINES 170047 TO 170087
// ----------------------------------------
L170047 | 			}
L170048 | 		}
L170049 | 
L170050 | 		[ObserversRpc(RunLocally = true)]
L170051 | 		private void SetStage(EStage stage)
L170052 | 		{
L170053 | 			RpcWriter___Observers_SetStage_2502303021(stage);
L170054 | 			RpcLogic___SetStage_2502303021(stage);
L170055 | 		}
L170056 | 
L170057 | 		private void RunRound(EStage stage)
L170058 | 		{
L170059 | 			SetBetMultiplier(GetNetBetMultiplier(stage - 1));
L170060 | 			StartCoroutine(RunRound());
L170061 | 			IEnumerator RunRound()
L170062 | 			{
L170063 | 				if (IsLocalPlayerInCurrentRound)
L170064 | 				{
L170065 | 					if (stage == EStage.RedOrBlack)
L170066 | 					{
L170067 | 						NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - LocalPlayerBet);
L170068 | 						Players.SetPlayerScore(Player.Local, Mathf.RoundToInt(LocalPlayerBet));
L170069 | 						base.LocalPlayerData.SetData("Ready", value: false);
L170070 | 					}
L170071 | 					PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(PlayCameraTransform.position, PlayCameraTransform.rotation, 0.8f);
L170072 | 					base.LocalPlayerData.SetData("Answer", 0f);
L170073 | 				}
L170074 | 				yield return new WaitForSeconds(0.4f);
L170075 | 				PlayingCard activeCard = null;
L170076 | 				if (stage == EStage.RedOrBlack)
L170077 | 				{
L170078 | 					activeCard = Cards[0];
L170079 | 				}
L170080 | 				else if (stage == EStage.HigherOrLower)
L170081 | 				{
L170082 | 					activeCard = Cards[1];
L170083 | 				}
L170084 | 				else if (stage == EStage.InsideOrOutside)
L170085 | 				{
L170086 | 					activeCard = Cards[2];
L170087 | 				}

// <<< END OF BLOCK: LINES 170047 TO 170087





// ################################################################################





// >>> START OF BLOCK: LINES 170152 TO 170192
// ----------------------------------------
L170152 | 		[ObserversRpc(RunLocally = true)]
L170153 | 		private void SetBetMultiplier(float multiplier)
L170154 | 		{
L170155 | 			RpcWriter___Observers_SetBetMultiplier_431000436(multiplier);
L170156 | 			RpcLogic___SetBetMultiplier_431000436(multiplier);
L170157 | 		}
L170158 | 
L170159 | 		[ObserversRpc(RunLocally = true)]
L170160 | 		private void EndGame()
L170161 | 		{
L170162 | 			RpcWriter___Observers_EndGame_2166136261();
L170163 | 			RpcLogic___EndGame_2166136261();
L170164 | 		}
L170165 | 
L170166 | 		public void RemoveLocalPlayerFromGame(bool payout, float cameraDelay = 0f)
L170167 | 		{
L170168 | 			RequestRemovePlayerFromCurrentRound(Player.Local.NetworkObject);
L170169 | 			Players.SetPlayerScore(Player.Local, 0);
L170170 | 			if (payout)
L170171 | 			{
L170172 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(LocalPlayerBet * LocalPlayerBetMultiplier);
L170173 | 			}
L170174 | 			if (onLocalPlayerExitRound != null)
L170175 | 			{
L170176 | 				onLocalPlayerExitRound();
L170177 | 			}
L170178 | 			StartCoroutine(Wait());
L170179 | 			IEnumerator Wait()
L170180 | 			{
L170181 | 				yield return new WaitForSeconds(cameraDelay);
L170182 | 				if (base.IsOpen && !IsLocalPlayerInCurrentRound)
L170183 | 				{
L170184 | 					PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(localDefaultCameraTransform.position, localDefaultCameraTransform.rotation, 0.5f);
L170185 | 				}
L170186 | 			}
L170187 | 		}
L170188 | 
L170189 | 		private bool IsCurrentRoundEmpty()
L170190 | 		{
L170191 | 			return playersInCurrentRound.Count == 0;
L170192 | 		}

// <<< END OF BLOCK: LINES 170152 TO 170192





// ################################################################################





// >>> START OF BLOCK: LINES 170976 TO 171037
// ----------------------------------------
L170976 | 			UpButton.SetMessage("Increase bet");
L170977 | 		}
L170978 | 
L170979 | 		private void UpInteracted()
L170980 | 		{
L170981 | 			if (onUpPressed != null)
L170982 | 			{
L170983 | 				onUpPressed.Invoke();
L170984 | 			}
L170985 | 			SendBetIndex(currentBetIndex + 1);
L170986 | 		}
L170987 | 
L170988 | 		private void HandleHovered()
L170989 | 		{
L170990 | 			if (IsSpinning)
L170991 | 			{
L170992 | 				HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
L170993 | 				return;
L170994 | 			}
L170995 | 			int num = currentBetAmount;
L170996 | 			if (NetworkSingleton<MoneyManager>.Instance.cashBalance < (float)num)
L170997 | 			{
L170998 | 				HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
L170999 | 				HandleIntObj.SetMessage("Insufficient cash");
L171000 | 			}
L171001 | 			else
L171002 | 			{
L171003 | 				HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
L171004 | 				HandleIntObj.SetMessage("Pull handle");
L171005 | 			}
L171006 | 		}
L171007 | 
L171008 | 		[Button]
L171009 | 		public void HandleInteracted()
L171010 | 		{
L171011 | 			if (!IsSpinning)
L171012 | 			{
L171013 | 				if (onHandlePulled != null)
L171014 | 				{
L171015 | 					onHandlePulled.Invoke();
L171016 | 				}
L171017 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-currentBetAmount);
L171018 | 				SendStartSpin(Player.Local.LocalConnection, currentBetAmount);
L171019 | 			}
L171020 | 		}
L171021 | 
L171022 | 		[ServerRpc(RequireOwnership = false, RunLocally = true)]
L171023 | 		private void SendBetIndex(int index)
L171024 | 		{
L171025 | 			RpcWriter___Server_SendBetIndex_3316948804(index);
L171026 | 			RpcLogic___SendBetIndex_3316948804(index);
L171027 | 		}
L171028 | 
L171029 | 		[ObserversRpc(RunLocally = true)]
L171030 | 		[TargetRpc]
L171031 | 		public void SetBetIndex(NetworkConnection conn, int index)
L171032 | 		{
L171033 | 			if ((object)conn == null)
L171034 | 			{
L171035 | 				RpcWriter___Observers_SetBetIndex_2681120339(conn, index);
L171036 | 				RpcLogic___SetBetIndex_2681120339(conn, index);
L171037 | 			}

// <<< END OF BLOCK: LINES 170976 TO 171037





// ################################################################################





// >>> START OF BLOCK: LINES 171079 TO 171119
// ----------------------------------------
L171079 | 			return EOutcome.NoWin;
L171080 | 		}
L171081 | 
L171082 | 		private int GetWinAmount(EOutcome outcome, int betAmount)
L171083 | 		{
L171084 | 			return outcome switch
L171085 | 			{
L171086 | 				EOutcome.Jackpot => betAmount * 100, 
L171087 | 				EOutcome.BigWin => betAmount * 25, 
L171088 | 				EOutcome.SmallWin => betAmount * 10, 
L171089 | 				EOutcome.MiniWin => betAmount * 2, 
L171090 | 				_ => 0, 
L171091 | 			};
L171092 | 		}
L171093 | 
L171094 | 		private void DisplayOutcome(EOutcome outcome, int winAmount)
L171095 | 		{
L171096 | 			TextMeshProUGUI[] winAmountLabels = WinAmountLabels;
L171097 | 			for (int i = 0; i < winAmountLabels.Length; i++)
L171098 | 			{
L171099 | 				winAmountLabels[i].text = MoneyManager.FormatAmount(winAmount);
L171100 | 			}
L171101 | 			switch (outcome)
L171102 | 			{
L171103 | 			case EOutcome.Jackpot:
L171104 | 			{
L171105 | 				ScreenAnimation.Play(JackpotAnimation.name);
L171106 | 				ParticleSystem[] jackpotParticles = JackpotParticles;
L171107 | 				for (int i = 0; i < jackpotParticles.Length; i++)
L171108 | 				{
L171109 | 					jackpotParticles[i].Play();
L171110 | 				}
L171111 | 				break;
L171112 | 			}
L171113 | 			case EOutcome.BigWin:
L171114 | 				ScreenAnimation.Play(BigWinAnimation.name);
L171115 | 				BigWinSound.Play();
L171116 | 				break;
L171117 | 			case EOutcome.SmallWin:
L171118 | 				ScreenAnimation.Play(SmallWinAnimation.name);
L171119 | 				SmallWinSound.Play();

// <<< END OF BLOCK: LINES 171079 TO 171119





// ################################################################################





// >>> START OF BLOCK: LINES 171291 TO 171331
// ----------------------------------------
L171291 | 					networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
L171292 | 				}
L171293 | 				else
L171294 | 				{
L171295 | 					UnityEngine.Debug.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
L171296 | 				}
L171297 | 			}
L171298 | 			else
L171299 | 			{
L171300 | 				Channel channel = Channel.Reliable;
L171301 | 				PooledWriter writer = WriterPool.GetWriter();
L171302 | 				writer.WriteInt32(index);
L171303 | 				SendObserversRpc(1u, writer, channel, DataOrderType.Default, bufferLast: false, excludeServer: false, excludeOwner: false);
L171304 | 				writer.Store();
L171305 | 			}
L171306 | 		}
L171307 | 
L171308 | 		public void RpcLogic___SetBetIndex_2681120339(NetworkConnection conn, int index)
L171309 | 		{
L171310 | 			currentBetIndex = Mathf.Clamp(index, 0, BetAmounts.Length - 1);
L171311 | 			BetAmountLabel.text = MoneyManager.FormatAmount(currentBetAmount);
L171312 | 		}
L171313 | 
L171314 | 		private void RpcReader___Observers_SetBetIndex_2681120339(PooledReader PooledReader0, Channel channel)
L171315 | 		{
L171316 | 			int index = PooledReader0.ReadInt32();
L171317 | 			if (base.IsClientInitialized && !base.IsHost)
L171318 | 			{
L171319 | 				RpcLogic___SetBetIndex_2681120339(null, index);
L171320 | 			}
L171321 | 		}
L171322 | 
L171323 | 		private void RpcWriter___Target_SetBetIndex_2681120339(NetworkConnection conn, int index)
L171324 | 		{
L171325 | 			if (!base.IsServerInitialized)
L171326 | 			{
L171327 | 				NetworkManager networkManager = base.NetworkManager;
L171328 | 				if ((object)networkManager == null)
L171329 | 				{
L171330 | 					networkManager = InstanceFinder.NetworkManager;
L171331 | 				}

// <<< END OF BLOCK: LINES 171291 TO 171331





// ################################################################################





// >>> START OF BLOCK: LINES 171463 TO 171503
// ----------------------------------------
L171463 | 						yield return new WaitForSeconds(0.3f);
L171464 | 					}
L171465 | 					yield return new WaitForSeconds(0.6f);
L171466 | 					if (outcome == EOutcome.Jackpot)
L171467 | 					{
L171468 | 						if (i == 0)
L171469 | 						{
L171470 | 							JackpotSound.Play();
L171471 | 						}
L171472 | 						else
L171473 | 						{
L171474 | 							yield return new WaitForSeconds(0.35f);
L171475 | 						}
L171476 | 					}
L171477 | 					Reels[i].Stop(symbols[i]);
L171478 | 				}
L171479 | 				SpinLoop.Stop();
L171480 | 				int winAmount = GetWinAmount(outcome, betAmount);
L171481 | 				if (spinner.IsLocalClient)
L171482 | 				{
L171483 | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(winAmount);
L171484 | 				}
L171485 | 				DisplayOutcome(outcome, winAmount);
L171486 | 				IsSpinning = false;
L171487 | 			}
L171488 | 		}
L171489 | 
L171490 | 		private void RpcReader___Observers_StartSpin_2659526290(PooledReader PooledReader0, Channel channel)
L171491 | 		{
L171492 | 			NetworkConnection spinner = PooledReader0.ReadNetworkConnection();
L171493 | 			ESymbol[] symbols = GeneratedReaders___Internal.Read___ScheduleOne.Casino.SlotMachine/ESymbol[]FishNet.Serializing.Generateds(PooledReader0);
L171494 | 			int betAmount = PooledReader0.ReadInt32();
L171495 | 			if (base.IsClientInitialized && !base.IsHost)
L171496 | 			{
L171497 | 				RpcLogic___StartSpin_2659526290(spinner, symbols, betAmount);
L171498 | 			}
L171499 | 		}
L171500 | 
L171501 | 		private void Awake_UserLogic_ScheduleOne.Casino.SlotMachine_Assembly-CSharp.dll()
L171502 | 		{
L171503 | 			DownButton.onHovered.AddListener(DownHovered);

// <<< END OF BLOCK: LINES 171463 TO 171503





// ################################################################################





// >>> START OF BLOCK: LINES 171776 TO 171822
// ----------------------------------------
L171776 | 			}
L171777 | 			CurrentGame = null;
L171778 | 			PlayerDisplay.Unbind();
L171779 | 			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
L171780 | 			Canvas.enabled = false;
L171781 | 		}
L171782 | 
L171783 | 		private void BetSliderChanged(float newValue)
L171784 | 		{
L171785 | 			CurrentGame.SetLocalPlayerBet(GetBetFromSliderValue(newValue));
L171786 | 			RefreshDisplayedBet();
L171787 | 		}
L171788 | 
L171789 | 		private float GetBetFromSliderValue(float sliderVal)
L171790 | 		{
L171791 | 			return Mathf.Lerp(10f, 1000f, Mathf.Pow(sliderVal, 2f));
L171792 | 		}
L171793 | 
L171794 | 		private void RefreshDisplayedBet()
L171795 | 		{
L171796 | 			BetAmount.text = MoneyManager.FormatAmount(CurrentGame.LocalPlayerBet);
L171797 | 			BetSlider.SetValueWithoutNotify(Mathf.Sqrt(Mathf.InverseLerp(10f, 1000f, CurrentGame.LocalPlayerBet)));
L171798 | 		}
L171799 | 
L171800 | 		private void RefreshReadyButton()
L171801 | 		{
L171802 | 			if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= CurrentGame.LocalPlayerBet)
L171803 | 			{
L171804 | 				ReadyButton.interactable = true;
L171805 | 				BetAmount.color = new Color32(84, 231, 23, byte.MaxValue);
L171806 | 			}
L171807 | 			else
L171808 | 			{
L171809 | 				ReadyButton.interactable = false;
L171810 | 				BetAmount.color = new Color32(231, 52, 23, byte.MaxValue);
L171811 | 			}
L171812 | 			if (CurrentGame.LocalPlayerData.GetData<bool>("Ready"))
L171813 | 			{
L171814 | 				ReadyLabel.text = "Cancel";
L171815 | 			}
L171816 | 			else
L171817 | 			{
L171818 | 				ReadyLabel.text = "Ready";
L171819 | 			}
L171820 | 		}
L171821 | 
L171822 | 		private void LocalPlayerReadyForInput()

// <<< END OF BLOCK: LINES 171776 TO 171822





// ################################################################################





// >>> START OF BLOCK: LINES 171863 TO 171903
// ----------------------------------------
L171863 | 				InputContainerAnimation.Play(InputContainerFadeOut.name);
L171864 | 			}
L171865 | 		}
L171866 | 
L171867 | 		private void ReadyButtonClicked()
L171868 | 		{
L171869 | 			CurrentGame.ToggleLocalPlayerReady();
L171870 | 		}
L171871 | 
L171872 | 		private void OnLocalPlayerBust()
L171873 | 		{
L171874 | 			if (onBust != null)
L171875 | 			{
L171876 | 				onBust.Invoke();
L171877 | 			}
L171878 | 		}
L171879 | 
L171880 | 		private void OnLocalPlayerRoundCompleted(BlackjackGameController.EPayoutType payout)
L171881 | 		{
L171882 | 			float payout2 = CurrentGame.GetPayout(CurrentGame.LocalPlayerBet, payout);
L171883 | 			PayoutLabel.text = MoneyManager.FormatAmount(payout2);
L171884 | 			switch (payout)
L171885 | 			{
L171886 | 			case BlackjackGameController.EPayoutType.None:
L171887 | 				if (!CurrentGame.IsLocalPlayerBust && onLose != null)
L171888 | 				{
L171889 | 					onLose.Invoke();
L171890 | 				}
L171891 | 				break;
L171892 | 			case BlackjackGameController.EPayoutType.Blackjack:
L171893 | 				PositiveOutcomeLabel.text = "Blackjack!";
L171894 | 				if (onBlackjack != null)
L171895 | 				{
L171896 | 					onBlackjack.Invoke();
L171897 | 				}
L171898 | 				break;
L171899 | 			case BlackjackGameController.EPayoutType.Win:
L171900 | 				PositiveOutcomeLabel.text = "Win!";
L171901 | 				if (onWin != null)
L171902 | 				{
L171903 | 					onWin.Invoke();

// <<< END OF BLOCK: LINES 171863 TO 171903





// ################################################################################





// >>> START OF BLOCK: LINES 171931 TO 171971
// ----------------------------------------
L171931 | 				if (player != null)
L171932 | 				{
L171933 | 					PlayerEntries[i].Find("Container/Name").GetComponent<TextMeshProUGUI>().text = player.PlayerName;
L171934 | 					PlayerEntries[i].Find("Container").gameObject.SetActive(value: true);
L171935 | 				}
L171936 | 				else
L171937 | 				{
L171938 | 					PlayerEntries[i].Find("Container").gameObject.SetActive(value: false);
L171939 | 				}
L171940 | 			}
L171941 | 			RefreshScores();
L171942 | 		}
L171943 | 
L171944 | 		public void RefreshScores()
L171945 | 		{
L171946 | 			int currentPlayerCount = BindedPlayers.CurrentPlayerCount;
L171947 | 			for (int i = 0; i < PlayerEntries.Length; i++)
L171948 | 			{
L171949 | 				if (currentPlayerCount > i)
L171950 | 				{
L171951 | 					PlayerEntries[i].Find("Container/Score").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(BindedPlayers.GetPlayerScore(BindedPlayers.GetPlayer(i)));
L171952 | 				}
L171953 | 			}
L171954 | 		}
L171955 | 
L171956 | 		public void Bind(CasinoGamePlayers players)
L171957 | 		{
L171958 | 			BindedPlayers = players;
L171959 | 			BindedPlayers.onPlayerListChanged.AddListener(RefreshPlayers);
L171960 | 			BindedPlayers.onPlayerScoresChanged.AddListener(RefreshScores);
L171961 | 			RefreshPlayers();
L171962 | 		}
L171963 | 
L171964 | 		public void Unbind()
L171965 | 		{
L171966 | 			if (!(BindedPlayers == null))
L171967 | 			{
L171968 | 				BindedPlayers.onPlayerListChanged.RemoveListener(RefreshPlayers);
L171969 | 				BindedPlayers.onPlayerScoresChanged.RemoveListener(RefreshScores);
L171970 | 				BindedPlayers = null;
L171971 | 			}

// <<< END OF BLOCK: LINES 171931 TO 171971





// ################################################################################





// >>> START OF BLOCK: LINES 172129 TO 172199
// ----------------------------------------
L172129 | 			}
L172130 | 			CurrentGame = null;
L172131 | 			PlayerDisplay.Unbind();
L172132 | 			Singleton<InputPromptsCanvas>.Instance.UnloadModule();
L172133 | 			Canvas.enabled = false;
L172134 | 		}
L172135 | 
L172136 | 		private void BetSliderChanged(float newValue)
L172137 | 		{
L172138 | 			CurrentGame.SetLocalPlayerBet(GetBetFromSliderValue(newValue));
L172139 | 			RefreshDisplayedBet();
L172140 | 		}
L172141 | 
L172142 | 		private float GetBetFromSliderValue(float sliderVal)
L172143 | 		{
L172144 | 			return Mathf.Lerp(10f, 500f, Mathf.Pow(sliderVal, 2f));
L172145 | 		}
L172146 | 
L172147 | 		private void RefreshDisplayedBet()
L172148 | 		{
L172149 | 			BetAmount.text = MoneyManager.FormatAmount(CurrentGame.LocalPlayerBet);
L172150 | 			BetSlider.SetValueWithoutNotify(Mathf.Sqrt(Mathf.InverseLerp(10f, 500f, CurrentGame.LocalPlayerBet)));
L172151 | 		}
L172152 | 
L172153 | 		private void RefreshReadyButton()
L172154 | 		{
L172155 | 			if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= CurrentGame.LocalPlayerBet)
L172156 | 			{
L172157 | 				ReadyButton.interactable = true;
L172158 | 				BetAmount.color = new Color32(84, 231, 23, byte.MaxValue);
L172159 | 			}
L172160 | 			else
L172161 | 			{
L172162 | 				ReadyButton.interactable = false;
L172163 | 				BetAmount.color = new Color32(231, 52, 23, byte.MaxValue);
L172164 | 			}
L172165 | 			if (CurrentGame.LocalPlayerData.GetData<bool>("Ready"))
L172166 | 			{
L172167 | 				ReadyLabel.text = "Cancel";
L172168 | 			}
L172169 | 			else
L172170 | 			{
L172171 | 				ReadyLabel.text = "Ready";
L172172 | 			}
L172173 | 		}
L172174 | 
L172175 | 		private void QuestionReady(string question, string[] answers)
L172176 | 		{
L172177 | 			QuestionLabel.text = question;
L172178 | 			SelectionIndicator.gameObject.SetActive(value: false);
L172179 | 			ForfeitLabel.text = "Forfeit and collect " + MoneyManager.FormatAmount(CurrentGame.MultipliedLocalPlayerBet, showDecimals: false, includeColor: true);
L172180 | 			QuestionCanvasGroup.interactable = true;
L172181 | 			for (int i = 0; i < AnswerButtons.Length; i++)
L172182 | 			{
L172183 | 				if (answers.Length > i)
L172184 | 				{
L172185 | 					AnswerLabels[i].text = answers[i];
L172186 | 					AnswerButtons[i].gameObject.SetActive(value: true);
L172187 | 				}
L172188 | 				else
L172189 | 				{
L172190 | 					AnswerButtons[i].gameObject.SetActive(value: false);
L172191 | 				}
L172192 | 			}
L172193 | 			QuestionContainerAnimation.Play(QuestionContainerFadeIn.name);
L172194 | 			TimerSlider.value = 1f;
L172195 | 			StartCoroutine(Routine());
L172196 | 			IEnumerator Routine()
L172197 | 			{
L172198 | 				while (CurrentGame != null && CurrentGame.IsQuestionActive)
L172199 | 				{

// <<< END OF BLOCK: LINES 172129 TO 172199





// ################################################################################





// >>> START OF BLOCK: LINES 175908 TO 175948
// ----------------------------------------
L175908 | 		private void InitializeDealQuest(NetworkConnection conn, CartelDealInfo dealInfo)
L175909 | 		{
L175910 | 			if ((object)conn == null)
L175911 | 			{
L175912 | 				RpcWriter___Observers_InitializeDealQuest_2137933519(conn, dealInfo);
L175913 | 				RpcLogic___InitializeDealQuest_2137933519(conn, dealInfo);
L175914 | 			}
L175915 | 			else
L175916 | 			{
L175917 | 				RpcWriter___Target_InitializeDealQuest_2137933519(conn, dealInfo);
L175918 | 			}
L175919 | 		}
L175920 | 
L175921 | 		private void SendRequestMessage(CartelDealInfo dealInfo)
L175922 | 		{
L175923 | 			MessageChain messageChain = RequestingNPC.DialogueHandler.Database.GetChain(EDialogueModule.Generic, "cartel_deal_request").GetMessageChain();
L175924 | 			for (int i = 0; i < messageChain.Messages.Count; i++)
L175925 | 			{
L175926 | 				string text = messageChain.Messages[i];
L175927 | 				text = text.Replace("<PRODUCT>", dealInfo.RequestedProductQuantity + "x " + Registry.GetItem(dealInfo.RequestedProductID).Name);
L175928 | 				text = text.Replace("<PAYMENT>", MoneyManager.FormatAmount(dealInfo.PaymentAmount));
L175929 | 				text = text.Replace("<DUE_DAYS>", 3.ToString());
L175930 | 				messageChain.Messages[i] = text;
L175931 | 			}
L175932 | 			RequestingNPC.MSGConversation.SendMessageChain(messageChain);
L175933 | 		}
L175934 | 
L175935 | 		private void SendOverdueMessage()
L175936 | 		{
L175937 | 			MessageChain messageChain = RequestingNPC.DialogueHandler.Database.GetChain(EDialogueModule.Generic, "cartel_deal_overdue").GetMessageChain();
L175938 | 			RequestingNPC.MSGConversation.SendMessageChain(messageChain);
L175939 | 		}
L175940 | 
L175941 | 		private void SendExpiryMessage()
L175942 | 		{
L175943 | 			MessageChain messageChain = RequestingNPC.DialogueHandler.Database.GetChain(EDialogueModule.Generic, "cartel_deal_expired").GetMessageChain();
L175944 | 			RequestingNPC.MSGConversation.SendMessageChain(messageChain);
L175945 | 		}
L175946 | 
L175947 | 		public void Load(CartelData data)
L175948 | 		{

// <<< END OF BLOCK: LINES 175908 TO 175948





// ################################################################################





// >>> START OF BLOCK: LINES 182114 TO 182198
// ----------------------------------------
L182114 | 		}
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

// <<< END OF BLOCK: LINES 182114 TO 182198





// ################################################################################





// >>> START OF BLOCK: LINES 192643 TO 192683
// ----------------------------------------
L192643 | 			}
L192644 | 			if (_lerpScale)
L192645 | 			{
L192646 | 				_transform.localScale = Vector3.Lerp(_min.localScale, _max.localScale, _currentLerpValue);
L192647 | 			}
L192648 | 		}
L192649 | 	}
L192650 | 	public class VehicleSaleSign : MonoBehaviour
L192651 | 	{
L192652 | 		public TextMeshPro NameLabel;
L192653 | 
L192654 | 		public TextMeshPro PriceLabel;
L192655 | 
L192656 | 		public LandVehicle VehiclePrefab;
L192657 | 
L192658 | 		private void Awake()
L192659 | 		{
L192660 | 			if (VehiclePrefab != null)
L192661 | 			{
L192662 | 				NameLabel.text = VehiclePrefab.VehicleName;
L192663 | 				PriceLabel.text = MoneyManager.FormatAmount(VehiclePrefab.VehiclePrice);
L192664 | 			}
L192665 | 			else
L192666 | 			{
L192667 | 				UnityEngine.Debug.LogWarning("VehicleSaleSign on " + base.gameObject.name + " has no VehiclePrefab assigned.");
L192668 | 			}
L192669 | 		}
L192670 | 	}
L192671 | 	public class VerticalLayoutGroupSetter : MonoBehaviour
L192672 | 	{
L192673 | 		public float LeftSpacing;
L192674 | 
L192675 | 		public float RightSpacing;
L192676 | 
L192677 | 		private VerticalLayoutGroup layoutGroup;
L192678 | 
L192679 | 		private void Awake()
L192680 | 		{
L192681 | 			layoutGroup = GetComponent<VerticalLayoutGroup>();
L192682 | 		}
L192683 | 

// <<< END OF BLOCK: LINES 192643 TO 192683





// ################################################################################





// >>> START OF BLOCK: LINES 198036 TO 198099
// ----------------------------------------
L198036 | 
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

// <<< END OF BLOCK: LINES 198036 TO 198099





// ################################################################################





// >>> START OF BLOCK: LINES 209955 TO 209995
// ----------------------------------------
L209955 | 
L209956 | 		public bool active { get; protected set; }
L209957 | 
L209958 | 		public float timeSinceActiveSet { get; protected set; }
L209959 | 
L209960 | 		protected virtual void Update()
L209961 | 		{
L209962 | 			timeSinceActiveSet += Time.deltaTime;
L209963 | 			if (timeSinceActiveSet > 3f)
L209964 | 			{
L209965 | 				active = false;
L209966 | 			}
L209967 | 			if (Group != null)
L209968 | 			{
L209969 | 				Group.alpha = Mathf.MoveTowards(Group.alpha, active ? 1f : 0f, Time.deltaTime / 0.25f);
L209970 | 			}
L209971 | 		}
L209972 | 
L209973 | 		public void SetBalance(float balance)
L209974 | 		{
L209975 | 			BalanceLabel.text = MoneyManager.FormatAmount(balance);
L209976 | 		}
L209977 | 
L209978 | 		public void Show()
L209979 | 		{
L209980 | 			active = true;
L209981 | 			timeSinceActiveSet = 0f;
L209982 | 		}
L209983 | 	}
L209984 | 	public class BlackOverlay : Singleton<BlackOverlay>
L209985 | 	{
L209986 | 		[Header("References")]
L209987 | 		public Canvas canvas;
L209988 | 
L209989 | 		public CanvasGroup group;
L209990 | 
L209991 | 		private Coroutine fadeRoutine;
L209992 | 
L209993 | 		public bool isShown { get; protected set; }
L209994 | 
L209995 | 		protected override void Awake()

// <<< END OF BLOCK: LINES 209955 TO 209995





// ################################################################################





// >>> START OF BLOCK: LINES 211235 TO 211276
// ----------------------------------------
L211235 | 		public void Open()
L211236 | 		{
L211237 | 			IsOpen = true;
L211238 | 			TitleLabel.text = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.CurrentDay.ToString() + ", Day " + (NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.ElapsedDays + 1);
L211239 | 			string[] items = itemsSoldByPlayer.Keys.ToArray();
L211240 | 			for (int i = 0; i < ProductEntries.Length; i++)
L211241 | 			{
L211242 | 				if (i < items.Length)
L211243 | 				{
L211244 | 					ItemDefinition item = Registry.GetItem(items[i]);
L211245 | 					ProductEntries[i].Find("Quantity").GetComponent<TextMeshProUGUI>().text = itemsSoldByPlayer[items[i]] + "x";
L211246 | 					ProductEntries[i].Find("Image").GetComponent<Image>().sprite = item.Icon;
L211247 | 					ProductEntries[i].Find("Name").GetComponent<TextMeshProUGUI>().text = item.Name;
L211248 | 					ProductEntries[i].gameObject.SetActive(value: true);
L211249 | 				}
L211250 | 				else
L211251 | 				{
L211252 | 					ProductEntries[i].gameObject.SetActive(value: false);
L211253 | 				}
L211254 | 			}
L211255 | 			PlayerEarningsLabel.text = MoneyManager.FormatAmount(moneyEarnedByPlayer);
L211256 | 			DealerEarningsLabel.text = MoneyManager.FormatAmount(moneyEarnedByDealers);
L211257 | 			XPGainedLabel.text = xpGained + " XP";
L211258 | 			Anim.Play("Daily summary 1");
L211259 | 			Canvas.enabled = true;
L211260 | 			Container.gameObject.SetActive(value: true);
L211261 | 			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
L211262 | 			StartCoroutine(Wait());
L211263 | 			IEnumerator Wait()
L211264 | 			{
L211265 | 				yield return new WaitForSeconds(0.1f * (float)items.Length + 0.5f);
L211266 | 				if (IsOpen)
L211267 | 				{
L211268 | 					Anim.Play("Daily summary 2");
L211269 | 				}
L211270 | 			}
L211271 | 		}
L211272 | 
L211273 | 		public void Close()
L211274 | 		{
L211275 | 			if (IsOpen)
L211276 | 			{

// <<< END OF BLOCK: LINES 211235 TO 211276





// ################################################################################





// >>> START OF BLOCK: LINES 211572 TO 211634
// ----------------------------------------
L211572 | 		{
L211573 | 			IsPlaying = true;
L211574 | 			if (routine != null)
L211575 | 			{
L211576 | 				StopCoroutine(routine);
L211577 | 			}
L211578 | 			routine = StartCoroutine(Routine());
L211579 | 			IEnumerator Routine()
L211580 | 			{
L211581 | 				Group.alpha = 0f;
L211582 | 				Canvas.enabled = true;
L211583 | 				Container.gameObject.SetActive(value: true);
L211584 | 				Title.text = "Deal completed for " + customer.NPC.fullName;
L211585 | 				PaymentLabel.text = "+$0";
L211586 | 				SatisfactionValueLabel.text = "0%";
L211587 | 				SatisfactionValueLabel.color = SatisfactionGradient.Evaluate(0f);
L211588 | 				for (int i = 0; i < BonusLabels.Length; i++)
L211589 | 				{
L211590 | 					if (bonuses.Count > i)
L211591 | 					{
L211592 | 						BonusLabels[i].text = "<color=#54E717>+" + MoneyManager.FormatAmount(bonuses[i].Amount) + "</color> " + bonuses[i].Title;
L211593 | 						BonusLabels[i].gameObject.SetActive(value: true);
L211594 | 					}
L211595 | 					else
L211596 | 					{
L211597 | 						BonusLabels[i].gameObject.SetActive(value: false);
L211598 | 					}
L211599 | 				}
L211600 | 				yield return new WaitForSeconds(0.2f);
L211601 | 				Anim.Play();
L211602 | 				SoundEffect.Play();
L211603 | 				RelationCircle.AssignNPC(customer.NPC);
L211604 | 				RelationCircle.SetUnlocked(NPCRelationData.EUnlockType.Recommendation, notify: false);
L211605 | 				RelationCircle.SetNotchPosition(originalRelationshipDelta);
L211606 | 				SetRelationshipLabel(originalRelationshipDelta);
L211607 | 				yield return new WaitForSeconds(0.2f);
L211608 | 				float paymentLerpTime = 1.5f;
L211609 | 				for (float i2 = 0f; i2 < paymentLerpTime; i2 += Time.deltaTime)
L211610 | 				{
L211611 | 					PaymentLabel.text = "+" + MoneyManager.FormatAmount(basePayment * (i2 / paymentLerpTime));
L211612 | 					yield return new WaitForEndOfFrame();
L211613 | 				}
L211614 | 				PaymentLabel.text = "+" + MoneyManager.FormatAmount(basePayment);
L211615 | 				yield return new WaitForSeconds(1.5f);
L211616 | 				float satisfactionLerpTime = 1f;
L211617 | 				for (float i2 = 0f; i2 < satisfactionLerpTime; i2 += Time.deltaTime)
L211618 | 				{
L211619 | 					SatisfactionValueLabel.color = SatisfactionGradient.Evaluate(i2 / satisfactionLerpTime * satisfaction);
L211620 | 					SatisfactionValueLabel.text = Mathf.Lerp(0f, satisfaction, i2 / satisfactionLerpTime).ToString("P0");
L211621 | 					yield return new WaitForEndOfFrame();
L211622 | 				}
L211623 | 				SatisfactionValueLabel.color = SatisfactionGradient.Evaluate(satisfaction);
L211624 | 				SatisfactionValueLabel.text = satisfaction.ToString("P0");
L211625 | 				yield return new WaitForSeconds(0.25f);
L211626 | 				float endDelta = customer.NPC.RelationData.RelationDelta;
L211627 | 				float lerpTime = Mathf.Abs(customer.NPC.RelationData.RelationDelta - originalRelationshipDelta);
L211628 | 				for (float i2 = 0f; i2 < lerpTime; i2 += Time.deltaTime)
L211629 | 				{
L211630 | 					float num = Mathf.Lerp(originalRelationshipDelta, endDelta, i2 / lerpTime);
L211631 | 					RelationCircle.SetNotchPosition(num);
L211632 | 					SetRelationshipLabel(num);
L211633 | 					yield return new WaitForEndOfFrame();
L211634 | 				}

// <<< END OF BLOCK: LINES 211572 TO 211634





// ################################################################################





// >>> START OF BLOCK: LINES 214041 TO 214102
// ----------------------------------------
L214041 | 		[SerializeField]
L214042 | 		protected GameObject timelineNotchPrefab;
L214043 | 
L214044 | 		[SerializeField]
L214045 | 		protected GameObject entryPrefab;
L214046 | 
L214047 | 		[Header("UI references")]
L214048 | 		[SerializeField]
L214049 | 		protected Canvas canvas;
L214050 | 
L214051 | 		private int selectedAmountToLaunder;
L214052 | 
L214053 | 		private Dictionary<LaunderingOperation, RectTransform> operationToNotch = new Dictionary<LaunderingOperation, RectTransform>();
L214054 | 
L214055 | 		private List<RectTransform> notches = new List<RectTransform>();
L214056 | 
L214057 | 		private bool ignoreSliderChange = true;
L214058 | 
L214059 | 		private Dictionary<LaunderingOperation, RectTransform> operationToEntry = new Dictionary<LaunderingOperation, RectTransform>();
L214060 | 
L214061 | 		protected int maxLaunderAmount => (int)Mathf.Min(business.appliedLaunderLimit, NetworkSingleton<MoneyManager>.Instance.cashBalance);
L214062 | 
L214063 | 		public Business business { get; private set; }
L214064 | 
L214065 | 		public bool isOpen
L214066 | 		{
L214067 | 			get
L214068 | 			{
L214069 | 				if (canvas != null)
L214070 | 				{
L214071 | 					return canvas.gameObject.activeSelf;
L214072 | 				}
L214073 | 				return false;
L214074 | 			}
L214075 | 		}
L214076 | 
L214077 | 		public void Initialize(Business bus)
L214078 | 		{
L214079 | 			business = bus;
L214080 | 			intObj.onHovered.AddListener(Hovered);
L214081 | 			intObj.onInteractStart.AddListener(Interacted);
L214082 | 			launderCapacityLabel.text = MoneyManager.FormatAmount(business.LaunderCapacity);
L214083 | 			canvas.gameObject.SetActive(value: false);
L214084 | 			noEntries.gameObject.SetActive(operationToEntry.Count == 0);
L214085 | 			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, (Action)delegate
L214086 | 			{
L214087 | 				canvas.worldCamera = PlayerSingleton<PlayerCamera>.Instance.Camera;
L214088 | 			});
L214089 | 			foreach (LaunderingOperation launderingOperation in business.LaunderingOperations)
L214090 | 			{
L214091 | 				CreateEntry(launderingOperation);
L214092 | 			}
L214093 | 			Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationStarted, new Action<LaunderingOperation>(CreateEntry));
L214094 | 			Business.onOperationStarted = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationStarted, new Action<LaunderingOperation>(UpdateCashStacks));
L214095 | 			Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationFinished, new Action<LaunderingOperation>(RemoveEntry));
L214096 | 			Business.onOperationFinished = (Action<LaunderingOperation>)Delegate.Combine(Business.onOperationFinished, new Action<LaunderingOperation>(UpdateCashStacks));
L214097 | 			GameInput.RegisterExitListener(Exit, 5);
L214098 | 			NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
L214099 | 			CloseAmountSelector();
L214100 | 		}
L214101 | 
L214102 | 		private void OnDestroy()

// <<< END OF BLOCK: LINES 214041 TO 214102





// ################################################################################





// >>> START OF BLOCK: LINES 214125 TO 214200
// ----------------------------------------
L214125 | 				if (amountSelectorScreen.gameObject.activeSelf)
L214126 | 				{
L214127 | 					exit.Used = true;
L214128 | 					CloseAmountSelector();
L214129 | 				}
L214130 | 				else if (exit.exitType == ExitType.Escape)
L214131 | 				{
L214132 | 					exit.Used = true;
L214133 | 					Close();
L214134 | 				}
L214135 | 			}
L214136 | 		}
L214137 | 
L214138 | 		protected void UpdateTimeline()
L214139 | 		{
L214140 | 			foreach (LaunderingOperation launderingOperation in business.LaunderingOperations)
L214141 | 			{
L214142 | 				if (!operationToNotch.ContainsKey(launderingOperation))
L214143 | 				{
L214144 | 					RectTransform component = UnityEngine.Object.Instantiate(timelineNotchPrefab, notchContainer).GetComponent<RectTransform>();
L214145 | 					component.Find("Amount").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(launderingOperation.amount);
L214146 | 					operationToNotch.Add(launderingOperation, component);
L214147 | 					notches.Add(component);
L214148 | 				}
L214149 | 			}
L214150 | 			List<RectTransform> list = (from x in operationToNotch
L214151 | 				where business.LaunderingOperations.Contains(x.Key)
L214152 | 				select x.Value).ToList();
L214153 | 			for (int num = 0; num < notches.Count; num++)
L214154 | 			{
L214155 | 				if (!list.Contains(notches[num]))
L214156 | 				{
L214157 | 					UnityEngine.Object.Destroy(notches[num].gameObject);
L214158 | 					notches.RemoveAt(num);
L214159 | 					num--;
L214160 | 				}
L214161 | 			}
L214162 | 			foreach (LaunderingOperation launderingOperation2 in business.LaunderingOperations)
L214163 | 			{
L214164 | 				operationToNotch[launderingOperation2].anchoredPosition = new Vector2(notchContainer.rect.width * (float)launderingOperation2.minutesSinceStarted / (float)launderingOperation2.completionTime_Minutes, operationToNotch[launderingOperation2].anchoredPosition.y);
L214165 | 			}
L214166 | 		}
L214167 | 
L214168 | 		protected void UpdateCurrentTotal()
L214169 | 		{
L214170 | 			currentTotalAmountLabel.text = MoneyManager.FormatAmount(business.currentLaunderTotal);
L214171 | 		}
L214172 | 
L214173 | 		private void CreateEntry(LaunderingOperation op)
L214174 | 		{
L214175 | 			if (!operationToEntry.ContainsKey(op))
L214176 | 			{
L214177 | 				RectTransform component = UnityEngine.Object.Instantiate(entryPrefab, entryContainer).GetComponent<RectTransform>();
L214178 | 				component.SetAsLastSibling();
L214179 | 				component.Find("BusinessLabel").GetComponent<TextMeshProUGUI>().text = op.business.PropertyName;
L214180 | 				component.Find("AmountLabel").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(op.amount);
L214181 | 				operationToEntry.Add(op, component);
L214182 | 				UpdateEntryTimes();
L214183 | 				if (noEntries != null)
L214184 | 				{
L214185 | 					noEntries.gameObject.SetActive(operationToEntry.Count == 0);
L214186 | 				}
L214187 | 			}
L214188 | 		}
L214189 | 
L214190 | 		private void RemoveEntry(LaunderingOperation op)
L214191 | 		{
L214192 | 			if (operationToEntry.ContainsKey(op))
L214193 | 			{
L214194 | 				RectTransform rectTransform = operationToEntry[op];
L214195 | 				if (rectTransform != null)
L214196 | 				{
L214197 | 					UnityEngine.Object.Destroy(rectTransform.gameObject);
L214198 | 				}
L214199 | 				operationToEntry.Remove(op);
L214200 | 				noEntries.gameObject.SetActive(operationToEntry.Count == 0);

// <<< END OF BLOCK: LINES 214125 TO 214200





// ################################################################################





// >>> START OF BLOCK: LINES 214228 TO 214303
// ----------------------------------------
L214228 | 		}
L214229 | 
L214230 | 		private void UpdateCashStacks(LaunderingOperation op)
L214231 | 		{
L214232 | 			float num = business.currentLaunderTotal;
L214233 | 			for (int i = 0; i < CashStacks.Length; i++)
L214234 | 			{
L214235 | 				if (num <= 0f)
L214236 | 				{
L214237 | 					CashStacks[i].ShowAmount(0f);
L214238 | 					continue;
L214239 | 				}
L214240 | 				float num2 = Mathf.Min(num, 1000f);
L214241 | 				CashStacks[i].ShowAmount(num2);
L214242 | 				num -= num2;
L214243 | 			}
L214244 | 		}
L214245 | 
L214246 | 		private void RefreshLaunderButton()
L214247 | 		{
L214248 | 			launderButton.interactable = business.currentLaunderTotal < business.LaunderCapacity && NetworkSingleton<MoneyManager>.Instance.cashBalance > 10f;
L214249 | 			if (business.currentLaunderTotal >= business.LaunderCapacity)
L214250 | 			{
L214251 | 				insufficientCashLabel.text = "The business is already at maximum laundering capacity.";
L214252 | 				insufficientCashLabel.gameObject.SetActive(value: true);
L214253 | 			}
L214254 | 			else if (NetworkSingleton<MoneyManager>.Instance.cashBalance <= 10f)
L214255 | 			{
L214256 | 				insufficientCashLabel.text = "You need at least " + MoneyManager.FormatAmount(10f) + " cash to launder.";
L214257 | 				insufficientCashLabel.gameObject.SetActive(value: true);
L214258 | 			}
L214259 | 			else
L214260 | 			{
L214261 | 				insufficientCashLabel.gameObject.SetActive(value: false);
L214262 | 			}
L214263 | 		}
L214264 | 
L214265 | 		public void OpenAmountSelector()
L214266 | 		{
L214267 | 			amountSelectorScreen.gameObject.SetActive(value: true);
L214268 | 			int num = (selectedAmountToLaunder = Mathf.Clamp(100, 10, maxLaunderAmount));
L214269 | 			amountSlider.minValue = 10f;
L214270 | 			amountSlider.maxValue = maxLaunderAmount;
L214271 | 			amountSlider.SetValueWithoutNotify(num);
L214272 | 			amountInputField.SetTextWithoutNotify(num.ToString());
L214273 | 		}
L214274 | 
L214275 | 		public void CloseAmountSelector()
L214276 | 		{
L214277 | 			amountSelectorScreen.gameObject.SetActive(value: false);
L214278 | 		}
L214279 | 
L214280 | 		public void ConfirmAmount()
L214281 | 		{
L214282 | 			int num = Mathf.Clamp(selectedAmountToLaunder, 10, maxLaunderAmount);
L214283 | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-num);
L214284 | 			business.StartLaunderingOperation(num);
L214285 | 			float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("LaunderingOperationsStarted");
L214286 | 			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LaunderingOperationsStarted", (value + 1f).ToString());
L214287 | 			UpdateTimeline();
L214288 | 			UpdateCurrentTotal();
L214289 | 			RefreshLaunderButton();
L214290 | 			CloseAmountSelector();
L214291 | 		}
L214292 | 
L214293 | 		public void SliderValueChanged()
L214294 | 		{
L214295 | 			if (ignoreSliderChange)
L214296 | 			{
L214297 | 				ignoreSliderChange = false;
L214298 | 				return;
L214299 | 			}
L214300 | 			selectedAmountToLaunder = (int)amountSlider.value;
L214301 | 			amountInputField.SetTextWithoutNotify(selectedAmountToLaunder.ToString());
L214302 | 		}
L214303 | 

// <<< END OF BLOCK: LINES 214228 TO 214303





// ################################################################################





// >>> START OF BLOCK: LINES 214785 TO 214825
// ----------------------------------------
L214785 | 			nameInputField.text = GenerateUniqueName(properties.ToArray(), drugType);
L214786 | 			Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
L214787 | 			PropertiesLabel.text = string.Empty;
L214788 | 			for (int i = 0; i < properties.Count; i++)
L214789 | 			{
L214790 | 				Effect effect = properties[i];
L214791 | 				if (PropertiesLabel.text.Length > 0)
L214792 | 				{
L214793 | 					PropertiesLabel.text += "\n";
L214794 | 				}
L214795 | 				if (i == 4 && properties.Count > 5)
L214796 | 				{
L214797 | 					int num = properties.Count - 5 + 1;
L214798 | 					TextMeshProUGUI propertiesLabel = PropertiesLabel;
L214799 | 					propertiesLabel.text = propertiesLabel.text + "+ " + num + " more...";
L214800 | 					break;
L214801 | 				}
L214802 | 				TextMeshProUGUI propertiesLabel2 = PropertiesLabel;
L214803 | 				propertiesLabel2.text = propertiesLabel2.text + "<color=#" + ColorUtility.ToHtmlStringRGBA(effect.LabelColor) + ">• " + effect.Name + "</color>";
L214804 | 			}
L214805 | 			MarketValueLabel.text = "Market Value: <color=#54E717>" + MoneyManager.FormatAmount(productMarketValue) + "</color>";
L214806 | 		}
L214807 | 
L214808 | 		public void Close()
L214809 | 		{
L214810 | 			canvas.enabled = false;
L214811 | 			Container.gameObject.SetActive(value: false);
L214812 | 			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
L214813 | 		}
L214814 | 
L214815 | 		public void RandomizeButtonClicked()
L214816 | 		{
L214817 | 			nameInputField.text = GenerateUniqueName();
L214818 | 		}
L214819 | 
L214820 | 		public void ConfirmButtonClicked()
L214821 | 		{
L214822 | 			if (onMixNamed != null)
L214823 | 			{
L214824 | 				onMixNamed(nameInputField.text);
L214825 | 			}

// <<< END OF BLOCK: LINES 214785 TO 214825





// ################################################################################





// >>> START OF BLOCK: LINES 215460 TO 215504
// ----------------------------------------
L215460 | 		private void PawnSlotChanged()
L215461 | 		{
L215462 | 			UpdateValueRangeLabels();
L215463 | 		}
L215464 | 
L215465 | 		private void UpdateValueRangeLabels()
L215466 | 		{
L215467 | 			float num = 0f;
L215468 | 			float num2 = 0f;
L215469 | 			for (int i = 0; i < PawnSlots.Length; i++)
L215470 | 			{
L215471 | 				if (PawnSlots[i].ItemInstance == null)
L215472 | 				{
L215473 | 					ValueRangeLabels[i].enabled = false;
L215474 | 					continue;
L215475 | 				}
L215476 | 				StorableItemDefinition storableItemDefinition = PawnSlots[i].ItemInstance.Definition as StorableItemDefinition;
L215477 | 				float num3 = storableItemDefinition.BasePurchasePrice * storableItemDefinition.ResellMultiplier * (float)PawnSlots[i].ItemInstance.Quantity;
L215478 | 				float num4 = num3 * 0.5f;
L215479 | 				float num5 = num3 * 2f;
L215480 | 				ValueRangeLabels[i].text = $"{MoneyManager.FormatAmount(num4)} - {MoneyManager.FormatAmount(num5)}";
L215481 | 				num += num4;
L215482 | 				num2 += num5;
L215483 | 			}
L215484 | 			TotalValueLabel.text = "Total: <color=#FFD755>" + $"{MoneyManager.FormatAmount(num)} - {MoneyManager.FormatAmount(num2)}" + "</color>";
L215485 | 		}
L215486 | 
L215487 | 		public void StartButtonPressed()
L215488 | 		{
L215489 | 			StartNegotiation();
L215490 | 		}
L215491 | 
L215492 | 		private void StartNegotiation()
L215493 | 		{
L215494 | 			if (CurrentState == EState.WaitingForOffer)
L215495 | 			{
L215496 | 				CurrentState = EState.Negotiating;
L215497 | 				CurrentNegotiationRound = 0;
L215498 | 				LastRefusedAmount = float.MaxValue;
L215499 | 				routine = StartCoroutine(NegotiationRoutine());
L215500 | 			}
L215501 | 			IEnumerator NegotiationRoutine()
L215502 | 			{
L215503 | 				Step1Animation.Play(FadeOutAnim.name);
L215504 | 				Think();

// <<< END OF BLOCK: LINES 215460 TO 215504





// ################################################################################





// >>> START OF BLOCK: LINES 215546 TO 215591
// ----------------------------------------
L215546 | 					case EPlayerResponse.Cancel:
L215547 | 						EndNegotiation();
L215548 | 						yield break;
L215549 | 					}
L215550 | 					CurrentNegotiationRound++;
L215551 | 				}
L215552 | 			}
L215553 | 		}
L215554 | 
L215555 | 		private void PlayShopResponse(EShopResponse response, float counter)
L215556 | 		{
L215557 | 			switch (response)
L215558 | 			{
L215559 | 			case EShopResponse.Accept:
L215560 | 			{
L215561 | 				string text2 = AcceptLines[UnityEngine.Random.Range(0, AcceptLines.Length)];
L215562 | 				PawnShopNPC.DialogueHandler.ShowWorldspaceDialogue(text2, 30f);
L215563 | 				break;
L215564 | 			}
L215565 | 			case EShopResponse.Counter:
L215566 | 				CounterLines[UnityEngine.Random.Range(0, CounterLines.Length)].Replace("<AMOUNT>", MoneyManager.FormatAmount(counter));
L215567 | 				break;
L215568 | 			case EShopResponse.Refusal:
L215569 | 			{
L215570 | 				string text = RefusalLines[UnityEngine.Random.Range(0, RefusalLines.Length)];
L215571 | 				text = text.Replace("<AMOUNT>", MoneyManager.FormatAmount(counter));
L215572 | 				PawnShopNPC.DialogueHandler.ShowWorldspaceDialogue(text, 30f);
L215573 | 				PawnShopNPC.PlayVO(EVOLineType.No);
L215574 | 				break;
L215575 | 			}
L215576 | 			}
L215577 | 		}
L215578 | 
L215579 | 		private EShopResponse EvaluateCounter(float lastShopOffer, float playerOffer, out float counterAmount, out float angerChange)
L215580 | 		{
L215581 | 			counterAmount = playerOffer;
L215582 | 			angerChange = 0f;
L215583 | 			float num = playerOffer / InitialShopOffer;
L215584 | 			float num2 = playerOffer / lastShopOffer;
L215585 | 			Console.Log("Original ratio: " + num + " - Last ratio: " + num2);
L215586 | 			float num3 = Mathf.Clamp01(2f - (num + num2) / 2f);
L215587 | 			float num4 = Mathf.Clamp(num3, 0f, 0.9f);
L215588 | 			num4 *= Mathf.Clamp01(1f - NPCAnger * 0.5f);
L215589 | 			num4 *= Mathf.Clamp01(1f - (float)CurrentNegotiationRound * 0.1f);
L215590 | 			Console.Log("Accept chance: " + num4);
L215591 | 			float num5 = UnityEngine.Random.Range(0f, 1f);

// <<< END OF BLOCK: LINES 215546 TO 215591





// ################################################################################





// >>> START OF BLOCK: LINES 215711 TO 215759
// ----------------------------------------
L215711 | 				Close(returnItemsToPlayer: true);
L215712 | 			}
L215713 | 		}
L215714 | 
L215715 | 		private void SetAngeredToday(bool angered)
L215716 | 		{
L215717 | 			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("PawnShopAngeredToday", angered.ToString());
L215718 | 		}
L215719 | 
L215720 | 		private void Think()
L215721 | 		{
L215722 | 			string text = ThinkLines[UnityEngine.Random.Range(0, ThinkLines.Length)];
L215723 | 			PawnShopNPC.DialogueHandler.ShowWorldspaceDialogue(text, 3f);
L215724 | 			PawnShopNPC.PlayVO(EVOLineType.Think);
L215725 | 		}
L215726 | 
L215727 | 		private void SetOffer(float amount)
L215728 | 		{
L215729 | 			Console.Log("Setting offer: " + amount);
L215730 | 			string text = OfferLines[UnityEngine.Random.Range(0, OfferLines.Length)];
L215731 | 			text = text.Replace("<AMOUNT>", MoneyManager.FormatAmount(amount));
L215732 | 			LastShopOffer = amount;
L215733 | 			SetSelectedPayment(amount);
L215734 | 			PawnShopNPC.DialogueHandler.ShowWorldspaceDialogue(text, 30f);
L215735 | 		}
L215736 | 
L215737 | 		private void FinalizeDeal(float amount)
L215738 | 		{
L215739 | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(amount, visualizeChange: true, playCashSound: true);
L215740 | 			string text = DealFinalizedLines[UnityEngine.Random.Range(0, DealFinalizedLines.Length)];
L215741 | 			PawnShopNPC.DialogueHandler.ShowWorldspaceDialogue(text, 5f);
L215742 | 			PawnShopNPC.PlayVO(EVOLineType.Acknowledge);
L215743 | 			Close(returnItemsToPlayer: false);
L215744 | 		}
L215745 | 
L215746 | 		private float GetTotalValue()
L215747 | 		{
L215748 | 			float num = 0f;
L215749 | 			ItemSlot[] pawnSlots = PawnSlots;
L215750 | 			foreach (ItemSlot itemSlot in pawnSlots)
L215751 | 			{
L215752 | 				if (itemSlot.ItemInstance != null)
L215753 | 				{
L215754 | 					num += GetItemValue(itemSlot.ItemInstance);
L215755 | 				}
L215756 | 			}
L215757 | 			return num;
L215758 | 		}
L215759 | 

// <<< END OF BLOCK: LINES 215711 TO 215759





// ################################################################################





// >>> START OF BLOCK: LINES 216924 TO 216977
// ----------------------------------------
L216924 | 			{
L216925 | 				action.Used = true;
L216926 | 				Close();
L216927 | 			}
L216928 | 		}
L216929 | 
L216930 | 		private void PlayerSpawned()
L216931 | 		{
L216932 | 			PatientNameLabel.text = Player.Local.PlayerName;
L216933 | 		}
L216934 | 
L216935 | 		public void Open()
L216936 | 		{
L216937 | 			isOpen = true;
L216938 | 			arrested = Player.Local.IsArrested;
L216939 | 			Canvas.enabled = true;
L216940 | 			Container.gameObject.SetActive(value: true);
L216941 | 			CanvasGroup.alpha = 1f;
L216942 | 			CanvasGroup.interactable = true;
L216943 | 			BillNumberLabel.text = UnityEngine.Random.Range(10000000, 100000000).ToString();
L216944 | 			float amount = Mathf.Min(250f, NetworkSingleton<MoneyManager>.Instance.cashBalance);
L216945 | 			PaidAmountLabel.text = MoneyManager.FormatAmount(amount, showDecimals: true);
L216946 | 			Singleton<PostProcessingManager>.Instance.SetBlur(1f);
L216947 | 			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
L216948 | 			Player.Deactivate(freeMouse: false);
L216949 | 		}
L216950 | 
L216951 | 		public void Close()
L216952 | 		{
L216953 | 			if (CanvasGroup.interactable && isOpen)
L216954 | 			{
L216955 | 				CanvasGroup.interactable = false;
L216956 | 				float num = Mathf.Min(250f, NetworkSingleton<MoneyManager>.Instance.cashBalance);
L216957 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - num);
L216958 | 				if (arrested)
L216959 | 				{
L216960 | 					CanvasGroup.alpha = 0f;
L216961 | 					Canvas.enabled = false;
L216962 | 					Container.gameObject.SetActive(value: false);
L216963 | 					PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(base.name);
L216964 | 					isOpen = false;
L216965 | 					Singleton<ArrestNoticeScreen>.Instance.Open();
L216966 | 				}
L216967 | 				else
L216968 | 				{
L216969 | 					StartCoroutine(CloseRoutine());
L216970 | 				}
L216971 | 			}
L216972 | 			IEnumerator CloseRoutine()
L216973 | 			{
L216974 | 				float lerpTime = 0.3f;
L216975 | 				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
L216976 | 				{
L216977 | 					CanvasGroup.alpha = Mathf.Lerp(1f, 0f, i / lerpTime);

// <<< END OF BLOCK: LINES 216924 TO 216977





// ################################################################################





// >>> START OF BLOCK: LINES 217108 TO 217198
// ----------------------------------------
L217108 | 				Player.Local.SendPassOutRecovery();
L217109 | 				Player.Local.Health.RecoverHealth(100f);
L217110 | 				Transform child = RecoveryPointsContainer.GetChild(UnityEngine.Random.Range(0, RecoveryPointsContainer.childCount));
L217111 | 				PlayerSingleton<PlayerMovement>.Instance.Teleport(child.position);
L217112 | 				Player.Local.transform.forward = child.forward;
L217113 | 				yield return new WaitForSeconds(0.5f);
L217114 | 				bool fadeBlur = false;
L217115 | 				if (Player.Local.IsArrested)
L217116 | 				{
L217117 | 					Singleton<ArrestNoticeScreen>.Instance.RecordCrimes();
L217118 | 					Player.Local.Free_Server();
L217119 | 					Singleton<ArrestNoticeScreen>.Instance.Open();
L217120 | 					PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
L217121 | 					yield return new WaitForSeconds(1f);
L217122 | 				}
L217123 | 				else
L217124 | 				{
L217125 | 					ContextLabel.text = "You awaken in a new location, unsure of how you got there.";
L217126 | 					if (cashLoss > 0f)
L217127 | 					{
L217128 | 						NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - cashLoss);
L217129 | 						ContextLabel.text = ContextLabel.text + "\n\n<color=#54E717>" + MoneyManager.FormatAmount(cashLoss) + "</color> is missing from your wallet.";
L217130 | 					}
L217131 | 					ContextLabel.gameObject.SetActive(value: true);
L217132 | 					for (float i = 0f; i < fadeTime; i += Time.deltaTime)
L217133 | 					{
L217134 | 						Group.alpha = Mathf.Lerp(0f, 1f, i / fadeTime);
L217135 | 						yield return new WaitForEndOfFrame();
L217136 | 					}
L217137 | 					fadeBlur = true;
L217138 | 					yield return new WaitForSeconds(4f);
L217139 | 					for (float i = 0f; i < fadeTime; i += Time.deltaTime)
L217140 | 					{
L217141 | 						Group.alpha = Mathf.Lerp(1f, 0f, i / fadeTime);
L217142 | 						yield return new WaitForEndOfFrame();
L217143 | 					}
L217144 | 					Group.alpha = 0f;
L217145 | 				}
L217146 | 				yield return new WaitForSeconds(1f);
L217147 | 				float lerpTime = 2f;
L217148 | 				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
L217149 | 				{
L217150 | 					Singleton<EyelidOverlay>.Instance.SetOpen(Mathf.Lerp(0f, 1f, i / lerpTime));
L217151 | 					if (fadeBlur)
L217152 | 					{
L217153 | 						Singleton<PostProcessingManager>.Instance.SetBlur(1f - i / lerpTime);
L217154 | 					}
L217155 | 					yield return new WaitForEndOfFrame();
L217156 | 				}
L217157 | 				Singleton<EyelidOverlay>.Instance.SetOpen(1f);
L217158 | 				if (fadeBlur)
L217159 | 				{
L217160 | 					Singleton<PostProcessingManager>.Instance.SetBlur(0f);
L217161 | 				}
L217162 | 				Close();
L217163 | 			}
L217164 | 		}
L217165 | 
L217166 | 		private void LoadSaveClicked()
L217167 | 		{
L217168 | 			Close();
L217169 | 		}
L217170 | 
L217171 | 		public void Open()
L217172 | 		{
L217173 | 			if (!isOpen)
L217174 | 			{
L217175 | 				isOpen = true;
L217176 | 				Singleton<EyelidOverlay>.Instance.Canvas.sortingOrder = 5;
L217177 | 				PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(base.name);
L217178 | 				cashLoss = Mathf.Min(UnityEngine.Random.Range(50f, 500f), NetworkSingleton<MoneyManager>.Instance.cashBalance);
L217179 | 				StartCoroutine(Routine());
L217180 | 			}
L217181 | 			IEnumerator Routine()
L217182 | 			{
L217183 | 				MainLabel.gameObject.SetActive(value: true);
L217184 | 				ContextLabel.gameObject.SetActive(value: false);
L217185 | 				yield return new WaitForSeconds(0.5f);
L217186 | 				Singleton<EyelidOverlay>.Instance.AutoUpdate = false;
L217187 | 				float lerpTime = 2f;
L217188 | 				float startOpenness = Singleton<EyelidOverlay>.Instance.CurrentOpen;
L217189 | 				float endOpenness = 0f;
L217190 | 				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
L217191 | 				{
L217192 | 					Singleton<EyelidOverlay>.Instance.SetOpen(Mathf.Lerp(startOpenness, endOpenness, i / lerpTime));
L217193 | 					Singleton<PostProcessingManager>.Instance.SetBlur(i / lerpTime);
L217194 | 					yield return new WaitForEndOfFrame();
L217195 | 				}
L217196 | 				Singleton<EyelidOverlay>.Instance.SetOpen(0f);
L217197 | 				Singleton<PostProcessingManager>.Instance.SetBlur(1f);
L217198 | 				yield return new WaitForSeconds(0.5f);

// <<< END OF BLOCK: LINES 217108 TO 217198





// ################################################################################





// >>> START OF BLOCK: LINES 219175 TO 219215
// ----------------------------------------
L219175 | 
L219176 | 		protected List<RectTransform> colorButtons = new List<RectTransform>();
L219177 | 
L219178 | 		protected Dictionary<EVehicleColor, RectTransform> colorToButton = new Dictionary<EVehicleColor, RectTransform>();
L219179 | 
L219180 | 		protected EVehicleColor selectedColor = EVehicleColor.White;
L219181 | 
L219182 | 		private Coroutine openCloseRoutine;
L219183 | 
L219184 | 		public bool IsOpen { get; private set; }
L219185 | 
L219186 | 		protected override void Awake()
L219187 | 		{
L219188 | 			base.Awake();
L219189 | 			GameInput.RegisterExitListener(Exit, 1);
L219190 | 		}
L219191 | 
L219192 | 		protected override void Start()
L219193 | 		{
L219194 | 			base.Start();
L219195 | 			confirmText_Online.text = "Confirm (" + MoneyManager.ApplyOnlineBalanceColor(MoneyManager.FormatAmount(repaintCost)) + ")";
L219196 | 			for (int i = 0; i < Singleton<VehicleColors>.Instance.colorLibrary.Count; i++)
L219197 | 			{
L219198 | 				RectTransform component = UnityEngine.Object.Instantiate(buttonPrefab, buttonContainer).GetComponent<RectTransform>();
L219199 | 				component.anchoredPosition = new Vector2((0.5f + (float)colorButtons.Count) * component.sizeDelta.x, component.anchoredPosition.y);
L219200 | 				component.Find("Image").GetComponent<Image>().color = Singleton<VehicleColors>.Instance.colorLibrary[i].UIColor;
L219201 | 				EVehicleColor c = Singleton<VehicleColors>.Instance.colorLibrary[i].color;
L219202 | 				colorButtons.Add(component);
L219203 | 				colorToButton.Add(c, component);
L219204 | 				component.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
L219205 | 				{
L219206 | 					ColorClicked(c);
L219207 | 				});
L219208 | 			}
L219209 | 		}
L219210 | 
L219211 | 		private void Exit(ExitAction action)
L219212 | 		{
L219213 | 			if (!action.Used && IsOpen && openCloseRoutine == null && action.exitType == ExitType.Escape)
L219214 | 			{
L219215 | 				action.Used = true;

// <<< END OF BLOCK: LINES 219175 TO 219215





// ################################################################################





// >>> START OF BLOCK: LINES 219272 TO 219325
// ----------------------------------------
L219272 | 				PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f);
L219273 | 				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0f);
L219274 | 				PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
L219275 | 				PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
L219276 | 				Singleton<InputPromptsCanvas>.Instance.UnloadModule();
L219277 | 				Singleton<BlackOverlay>.Instance.Close();
L219278 | 				openCloseRoutine = null;
L219279 | 			}
L219280 | 		}
L219281 | 
L219282 | 		public void ColorClicked(EVehicleColor col)
L219283 | 		{
L219284 | 			selectedColor = col;
L219285 | 			currentVehicle.ApplyColor(col);
L219286 | 			RefreshSelectionIndicator();
L219287 | 			UpdateConfirmButton();
L219288 | 		}
L219289 | 
L219290 | 		private void UpdateConfirmButton()
L219291 | 		{
L219292 | 			bool flag = NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= repaintCost;
L219293 | 			confirmButton_Online.interactable = flag && selectedColor != currentVehicle.OwnedColor;
L219294 | 		}
L219295 | 
L219296 | 		private void RefreshSelectionIndicator()
L219297 | 		{
L219298 | 			tempIndicator.position = colorToButton[selectedColor].position;
L219299 | 			permIndicator.position = colorToButton[currentVehicle.OwnedColor].position;
L219300 | 		}
L219301 | 
L219302 | 		public void ConfirmButtonClicked()
L219303 | 		{
L219304 | 			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Vehicle repaint", 0f - repaintCost, 1f, string.Empty);
L219305 | 			NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
L219306 | 			currentVehicle.SendOwnedColor(selectedColor);
L219307 | 			RefreshSelectionIndicator();
L219308 | 			if (onPaintPurchased != null)
L219309 | 			{
L219310 | 				onPaintPurchased.Invoke();
L219311 | 			}
L219312 | 			Close();
L219313 | 		}
L219314 | 	}
L219315 | 	public class VersionText : MonoBehaviour
L219316 | 	{
L219317 | 		private void Awake()
L219318 | 		{
L219319 | 			GetComponent<TextMeshProUGUI>().text = "v" + UnityEngine.Application.version;
L219320 | 		}
L219321 | 	}
L219322 | 	public class WorldspaceDialogueRenderer : MonoBehaviour
L219323 | 	{
L219324 | 		private const float FadeDist = 2f;
L219325 | 

// <<< END OF BLOCK: LINES 219272 TO 219325





// ################################################################################





// >>> START OF BLOCK: LINES 223314 TO 223354
// ----------------------------------------
L223314 | 		}
L223315 | 
L223316 | 		public void Send()
L223317 | 		{
L223318 | 			if (float.TryParse(PriceInput.text, out var result))
L223319 | 			{
L223320 | 				price = result;
L223321 | 			}
L223322 | 			price = Mathf.Clamp(price, 1f, 9999f);
L223323 | 			PriceInput.SetTextWithoutNotify(price.ToString());
L223324 | 			if (orderConfirmedCallback != null)
L223325 | 			{
L223326 | 				orderConfirmedCallback(selectedProduct, quantity, price);
L223327 | 			}
L223328 | 			Close();
L223329 | 		}
L223330 | 
L223331 | 		private void UpdateFairPrice()
L223332 | 		{
L223333 | 			float amount = selectedProduct.MarketValue * (float)quantity;
L223334 | 			FairPriceLabel.text = "Fair price: " + MoneyManager.FormatAmount(amount);
L223335 | 		}
L223336 | 
L223337 | 		private void SetProduct(ProductDefinition newProduct)
L223338 | 		{
L223339 | 			selectedProduct = newProduct;
L223340 | 			DisplayProduct(newProduct);
L223341 | 			UpdateFairPrice();
L223342 | 			ProductSelector.Close();
L223343 | 		}
L223344 | 
L223345 | 		private void DisplayProduct(ProductDefinition tempProduct)
L223346 | 		{
L223347 | 			ProductIcon.sprite = tempProduct.Icon;
L223348 | 			UpdatePriceQuantityLabel(tempProduct.Name);
L223349 | 		}
L223350 | 
L223351 | 		public void ChangeQuantity(int change)
L223352 | 		{
L223353 | 			quantity = Mathf.Clamp(quantity + change, 1, MaxQuantity);
L223354 | 			UpdatePriceQuantityLabel(selectedProduct.Name);

// <<< END OF BLOCK: LINES 223314 TO 223354





// ################################################################################





// >>> START OF BLOCK: LINES 223674 TO 223728
// ----------------------------------------
L223674 | 
L223675 | 		private Action<List<CartEntry>, float> orderConfirmedCallback;
L223676 | 
L223677 | 		private MSGConversation conversation;
L223678 | 
L223679 | 		public bool IsOpen { get; private set; }
L223680 | 
L223681 | 		private void Start()
L223682 | 		{
L223683 | 			GameInput.RegisterExitListener(Exit, 4);
L223684 | 			ConfirmButton.onClick.AddListener(ConfirmOrderPressed);
L223685 | 			ItemLimitContainer.gameObject.SetActive(value: true);
L223686 | 			Close();
L223687 | 		}
L223688 | 
L223689 | 		public void Open(string title, string subtitle, MSGConversation _conversation, List<Listing> listings, float _orderLimit, float debt, Action<List<CartEntry>, float> _orderConfirmedCallback)
L223690 | 		{
L223691 | 			IsOpen = true;
L223692 | 			TitleLabel.text = title;
L223693 | 			SubtitleLabel.text = subtitle;
L223694 | 			OrderLimitLabel.text = MoneyManager.FormatAmount(_orderLimit);
L223695 | 			DebtLabel.text = MoneyManager.FormatAmount(debt);
L223696 | 			orderLimit = _orderLimit;
L223697 | 			conversation = _conversation;
L223698 | 			MSGConversation mSGConversation = conversation;
L223699 | 			mSGConversation.onMessageRendered = (Action)Delegate.Combine(mSGConversation.onMessageRendered, new Action(Close));
L223700 | 			orderConfirmedCallback = _orderConfirmedCallback;
L223701 | 			_items.Clear();
L223702 | 			_items.AddRange(listings);
L223703 | 			foreach (Listing entry in listings)
L223704 | 			{
L223705 | 				RectTransform rectTransform = UnityEngine.Object.Instantiate(EntryPrefab, EntryContainer);
L223706 | 				rectTransform.Find("Icon").GetComponent<Image>().sprite = entry.Item.Icon;
L223707 | 				rectTransform.Find("Name").GetComponent<Text>().text = entry.Item.Name;
L223708 | 				rectTransform.Find("Price").GetComponent<Text>().text = MoneyManager.FormatAmount(entry.Price);
L223709 | 				rectTransform.Find("Quantity").GetComponent<Text>().text = "0";
L223710 | 				StorableItemDefinition item = entry.Item;
L223711 | 				if (!item.RequiresLevelToPurchase || NetworkSingleton<LevelManager>.Instance.GetFullRank() >= item.RequiredRank)
L223712 | 				{
L223713 | 					rectTransform.Find("Quantity/Remove").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
L223714 | 					{
L223715 | 						ChangeListingQuantity(entry, -1);
L223716 | 					});
L223717 | 					rectTransform.Find("Quantity/Add").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate
L223718 | 					{
L223719 | 						ChangeListingQuantity(entry, 1);
L223720 | 					});
L223721 | 					rectTransform.Find("Locked").gameObject.SetActive(value: false);
L223722 | 				}
L223723 | 				else
L223724 | 				{
L223725 | 					rectTransform.Find("Locked/Title").GetComponent<Text>().text = "Unlocks at " + item.RequiredRank.ToString();
L223726 | 					rectTransform.Find("Locked").gameObject.SetActive(value: true);
L223727 | 				}
L223728 | 				_entries.Add(rectTransform);

// <<< END OF BLOCK: LINES 223674 TO 223728





// ################################################################################





// >>> START OF BLOCK: LINES 223781 TO 223821
// ----------------------------------------
L223781 | 		{
L223782 | 			orderConfirmedCallback(_cart, GetOrderTotal(out var _));
L223783 | 			Close();
L223784 | 		}
L223785 | 
L223786 | 		private bool CanConfirmOrder()
L223787 | 		{
L223788 | 			int itemCount;
L223789 | 			float orderTotal = GetOrderTotal(out itemCount);
L223790 | 			if (orderTotal > 0f && orderTotal <= orderLimit)
L223791 | 			{
L223792 | 				return itemCount <= 10;
L223793 | 			}
L223794 | 			return false;
L223795 | 		}
L223796 | 
L223797 | 		private void UpdateOrderTotal()
L223798 | 		{
L223799 | 			int itemCount;
L223800 | 			float orderTotal = GetOrderTotal(out itemCount);
L223801 | 			OrderTotalLabel.text = MoneyManager.FormatAmount(orderTotal);
L223802 | 			OrderTotalLabel.color = ((orderTotal <= orderLimit) ? ValidAmountColor : InvalidAmountColor);
L223803 | 			ItemLimitLabel.text = itemCount + "/" + 10;
L223804 | 			ItemLimitLabel.color = ((itemCount <= 10) ? Color.black : InvalidAmountColor);
L223805 | 		}
L223806 | 
L223807 | 		private float GetOrderTotal(out int itemCount)
L223808 | 		{
L223809 | 			float num = 0f;
L223810 | 			itemCount = 0;
L223811 | 			foreach (CartEntry item in _cart)
L223812 | 			{
L223813 | 				num += item.Listing.Price * (float)item.Quantity;
L223814 | 				itemCount += item.Quantity;
L223815 | 			}
L223816 | 			return num;
L223817 | 		}
L223818 | 	}
L223819 | 	public class AppsCanvas : PlayerSingleton<AppsCanvas>
L223820 | 	{
L223821 | 		[Header("References")]

// <<< END OF BLOCK: LINES 223781 TO 223821





// ################################################################################





// >>> START OF BLOCK: LINES 224641 TO 224681
// ----------------------------------------
L224641 | 
L224642 | 		public void Awake()
L224643 | 		{
L224644 | 			ListedForSale.onValueChanged.AddListener(delegate
L224645 | 			{
L224646 | 				ListingToggled();
L224647 | 			});
L224648 | 			ValueLabel.onEndEdit.AddListener(delegate(string value)
L224649 | 			{
L224650 | 				PriceSubmitted(value);
L224651 | 			});
L224652 | 		}
L224653 | 
L224654 | 		public void SetActiveProduct(ProductDefinition productDefinition)
L224655 | 		{
L224656 | 			ActiveProduct = productDefinition;
L224657 | 			bool flag = ProductManager.DiscoveredProducts.Contains(productDefinition);
L224658 | 			if (ActiveProduct != null)
L224659 | 			{
L224660 | 				NameLabel.text = productDefinition.Name;
L224661 | 				SuggestedPriceLabel.text = "Suggested: " + MoneyManager.FormatAmount(productDefinition.MarketValue);
L224662 | 				UpdatePrice();
L224663 | 				if (flag)
L224664 | 				{
L224665 | 					DescLabel.text = productDefinition.Description;
L224666 | 				}
L224667 | 				else
L224668 | 				{
L224669 | 					DescLabel.text = "???";
L224670 | 				}
L224671 | 				for (int i = 0; i < PropertyLabels.Length; i++)
L224672 | 				{
L224673 | 					if (productDefinition.Properties.Count > i)
L224674 | 					{
L224675 | 						PropertyLabels[i].text = "•  " + productDefinition.Properties[i].Name;
L224676 | 						PropertyLabels[i].color = productDefinition.Properties[i].LabelColor;
L224677 | 						PropertyLabels[i].gameObject.SetActive(value: true);
L224678 | 					}
L224679 | 					else
L224680 | 					{
L224681 | 						PropertyLabels[i].gameObject.SetActive(value: false);

// <<< END OF BLOCK: LINES 224641 TO 224681





// ################################################################################





// >>> START OF BLOCK: LINES 225309 TO 225349
// ----------------------------------------
L225309 | 		public bool IsAvailable { get; private set; }
L225310 | 
L225311 | 		private void Start()
L225312 | 		{
L225313 | 			MatchingShop = ShopInterface.AllShops.Find((ShopInterface x) => x.ShopName == MatchingShopInterfaceName);
L225314 | 			if (MatchingShop == null)
L225315 | 			{
L225316 | 				UnityEngine.Debug.LogError("Could not find shop interface with name " + MatchingShopInterfaceName);
L225317 | 				return;
L225318 | 			}
L225319 | 			foreach (ShopListing listing in MatchingShop.Listings)
L225320 | 			{
L225321 | 				if (listing.CanBeDelivered)
L225322 | 				{
L225323 | 					ListingEntry listingEntry = UnityEngine.Object.Instantiate(ListingEntryPrefab, ListingContainer);
L225324 | 					listingEntry.Initialize(listing);
L225325 | 					listingEntry.onQuantityChanged.AddListener(RefreshCart);
L225326 | 					listingEntries.Add(listingEntry);
L225327 | 				}
L225328 | 			}
L225329 | 			DeliveryFeeLabel.text = MoneyManager.FormatAmount(DeliveryFee);
L225330 | 			int num = Mathf.CeilToInt((float)listingEntries.Count / 2f);
L225331 | 			ContentsContainer.sizeDelta = new Vector2(ContentsContainer.sizeDelta.x, 230f + (float)num * 60f);
L225332 | 			HeaderButton.onClick.AddListener(delegate
L225333 | 			{
L225334 | 				SetIsExpanded(!IsExpanded);
L225335 | 			});
L225336 | 			OrderButton.onClick.AddListener(OrderPressed);
L225337 | 			DestinationDropdown.onValueChanged.AddListener(DestinationDropdownSelected);
L225338 | 			LoadingDockDropdown.onValueChanged.AddListener(LoadingDockDropdownSelected);
L225339 | 			SetIsExpanded(expanded: false);
L225340 | 			if (AvailableByDefault)
L225341 | 			{
L225342 | 				SetIsAvailable();
L225343 | 			}
L225344 | 			else
L225345 | 			{
L225346 | 				base.gameObject.SetActive(value: false);
L225347 | 			}
L225348 | 			MatchingShop.DeliveryVehicle.Deactivate();
L225349 | 		}

// <<< END OF BLOCK: LINES 225309 TO 225349





// ################################################################################





// >>> START OF BLOCK: LINES 225376 TO 225471
// ----------------------------------------
L225376 | 		{
L225377 | 			if (!CanOrder(out var reason))
L225378 | 			{
L225379 | 				UnityEngine.Debug.LogWarning("Cannot order: " + reason);
L225380 | 				return;
L225381 | 			}
L225382 | 			float orderTotal = GetOrderTotal();
L225383 | 			List<StringIntPair> list = new List<StringIntPair>();
L225384 | 			foreach (ListingEntry listingEntry in listingEntries)
L225385 | 			{
L225386 | 				if (listingEntry.SelectedQuantity > 0)
L225387 | 				{
L225388 | 					list.Add(new StringIntPair(listingEntry.MatchingListing.Item.ID, listingEntry.SelectedQuantity));
L225389 | 					NetworkSingleton<VariableDatabase>.Instance.NotifyItemAcquired(listingEntry.MatchingListing.Item.ID, listingEntry.SelectedQuantity);
L225390 | 				}
L225391 | 			}
L225392 | 			int orderItemCount = GetOrderItemCount();
L225393 | 			int timeUntilArrival = Mathf.RoundToInt(Mathf.Lerp(60f, 360f, Mathf.Clamp01((float)orderItemCount / 160f)));
L225394 | 			DeliveryInstance delivery = new DeliveryInstance(GUIDManager.GenerateUniqueGUID().ToString(), MatchingShopInterfaceName, destinationProperty.PropertyCode, loadingDockIndex - 1, list.ToArray(), EDeliveryStatus.InTransit, timeUntilArrival);
L225395 | 			NetworkSingleton<DeliveryManager>.Instance.SendDelivery(delivery);
L225396 | 			NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Delivery from " + MatchingShop.ShopName, 0f - orderTotal, 1f, string.Empty);
L225397 | 			PlayerSingleton<DeliveryApp>.Instance.PlayOrderSubmittedAnim();
L225398 | 			ResetCart();
L225399 | 		}
L225400 | 
L225401 | 		public void RefreshShop()
L225402 | 		{
L225403 | 			RefreshCart();
L225404 | 			RefreshOrderButton();
L225405 | 			RefreshDestinationUI();
L225406 | 			RefreshLoadingDockUI();
L225407 | 			RefreshEntryOrder();
L225408 | 			RefreshEntriesLocked();
L225409 | 		}
L225410 | 
L225411 | 		public void ResetCart()
L225412 | 		{
L225413 | 			foreach (ListingEntry listingEntry in listingEntries)
L225414 | 			{
L225415 | 				listingEntry.SetQuantity(0, notify: false);
L225416 | 			}
L225417 | 			RefreshCart();
L225418 | 			RefreshOrderButton();
L225419 | 		}
L225420 | 
L225421 | 		private void RefreshCart()
L225422 | 		{
L225423 | 			ItemTotalLabel.text = MoneyManager.FormatAmount(GetCartCost());
L225424 | 			OrderTotalLabel.text = MoneyManager.FormatAmount(GetOrderTotal());
L225425 | 		}
L225426 | 
L225427 | 		private void RefreshOrderButton()
L225428 | 		{
L225429 | 			if (CanOrder(out var reason))
L225430 | 			{
L225431 | 				OrderButton.interactable = true;
L225432 | 				OrderButtonNote.enabled = false;
L225433 | 			}
L225434 | 			else
L225435 | 			{
L225436 | 				OrderButton.interactable = false;
L225437 | 				OrderButtonNote.text = reason;
L225438 | 				OrderButtonNote.enabled = true;
L225439 | 			}
L225440 | 		}
L225441 | 
L225442 | 		public bool CanOrder(out string reason)
L225443 | 		{
L225444 | 			reason = string.Empty;
L225445 | 			if (HasActiveDelivery())
L225446 | 			{
L225447 | 				reason = "Delivery already in progress";
L225448 | 				return false;
L225449 | 			}
L225450 | 			float cartCost = GetCartCost();
L225451 | 			if (GetOrderTotal() > NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance)
L225452 | 			{
L225453 | 				reason = "Insufficient online balance";
L225454 | 				return false;
L225455 | 			}
L225456 | 			if (destinationProperty == null)
L225457 | 			{
L225458 | 				reason = "Select a destination";
L225459 | 				return false;
L225460 | 			}
L225461 | 			if (destinationProperty.LoadingDockCount == 0)
L225462 | 			{
L225463 | 				reason = "Selected destination has no loading docks";
L225464 | 				return false;
L225465 | 			}
L225466 | 			if (loadingDockIndex == 0)
L225467 | 			{
L225468 | 				reason = "Select a loading dock";
L225469 | 				return false;
L225470 | 			}
L225471 | 			if (!WillCartFitInVehicle())

// <<< END OF BLOCK: LINES 225376 TO 225471





// ################################################################################





// >>> START OF BLOCK: LINES 225709 TO 225749
// ----------------------------------------
L225709 | 
L225710 | 		public InputField QuantityInput;
L225711 | 
L225712 | 		public UnityEngine.UI.Button IncrementButton;
L225713 | 
L225714 | 		public UnityEngine.UI.Button DecrementButton;
L225715 | 
L225716 | 		public RectTransform LockedContainer;
L225717 | 
L225718 | 		public UnityEvent onQuantityChanged;
L225719 | 
L225720 | 		public ShopListing MatchingListing { get; private set; }
L225721 | 
L225722 | 		public int SelectedQuantity { get; private set; }
L225723 | 
L225724 | 		public void Initialize(ShopListing match)
L225725 | 		{
L225726 | 			MatchingListing = match;
L225727 | 			Icon.sprite = MatchingListing.Item.Icon;
L225728 | 			ItemNameLabel.text = MatchingListing.Item.Name;
L225729 | 			ItemPriceLabel.text = MoneyManager.FormatAmount(MatchingListing.Price);
L225730 | 			QuantityInput.onSubmit.AddListener(OnQuantityInputSubmitted);
L225731 | 			QuantityInput.onEndEdit.AddListener(delegate
L225732 | 			{
L225733 | 				ValidateInput();
L225734 | 			});
L225735 | 			IncrementButton.onClick.AddListener(delegate
L225736 | 			{
L225737 | 				ChangeQuantity(1);
L225738 | 			});
L225739 | 			DecrementButton.onClick.AddListener(delegate
L225740 | 			{
L225741 | 				ChangeQuantity(-1);
L225742 | 			});
L225743 | 			QuantityInput.SetTextWithoutNotify(SelectedQuantity.ToString());
L225744 | 			RefreshLocked();
L225745 | 		}
L225746 | 
L225747 | 		public void RefreshLocked()
L225748 | 		{
L225749 | 			if (MatchingListing.Item.IsUnlocked)

// <<< END OF BLOCK: LINES 225709 TO 225749





// ################################################################################





// >>> START OF BLOCK: LINES 225873 TO 225913
// ----------------------------------------
L225873 | 				SetDisplayedDealer(dealers[0]);
L225874 | 			}
L225875 | 			else
L225876 | 			{
L225877 | 				NoDealersLabel.gameObject.SetActive(value: true);
L225878 | 				Content.gameObject.SetActive(value: false);
L225879 | 			}
L225880 | 			base.SetOpen(open);
L225881 | 		}
L225882 | 
L225883 | 		public void SetDisplayedDealer(Dealer dealer)
L225884 | 		{
L225885 | 			if (dealer == null)
L225886 | 			{
L225887 | 				Console.LogError("Cannot set displayed dealer to null!");
L225888 | 				return;
L225889 | 			}
L225890 | 			SelectedDealer = dealer;
L225891 | 			SelectorImage.sprite = dealer.MugshotSprite;
L225892 | 			SelectorTitle.text = dealer.fullName;
L225893 | 			CashLabel.text = MoneyManager.FormatAmount(dealer.Cash);
L225894 | 			CutLabel.text = Mathf.RoundToInt(dealer.Cut * 100f) + "%";
L225895 | 			HomeLabel.text = dealer.HomeName;
L225896 | 			Dictionary<string, int> dictionary = new Dictionary<string, int>();
L225897 | 			List<string> list = new List<string>();
L225898 | 			foreach (ItemSlot allSlot in dealer.GetAllSlots())
L225899 | 			{
L225900 | 				if (allSlot.Quantity != 0)
L225901 | 				{
L225902 | 					int num = allSlot.Quantity;
L225903 | 					if (allSlot.ItemInstance is ProductItemInstance)
L225904 | 					{
L225905 | 						num *= ((ProductItemInstance)allSlot.ItemInstance).Amount;
L225906 | 					}
L225907 | 					if (list.Contains(allSlot.ItemInstance.ID))
L225908 | 					{
L225909 | 						dictionary[allSlot.ItemInstance.ID] += num;
L225910 | 						continue;
L225911 | 					}
L225912 | 					list.Add(allSlot.ItemInstance.ID);
L225913 | 					dictionary.Add(allSlot.ItemInstance.ID, num);

// <<< END OF BLOCK: LINES 225873 TO 225913





// ################################################################################





// >>> START OF BLOCK: LINES 231169 TO 231209
// ----------------------------------------
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

// <<< END OF BLOCK: LINES 231169 TO 231209





// ################################################################################





// >>> START OF BLOCK: LINES 231516 TO 231556
// ----------------------------------------
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

// <<< END OF BLOCK: LINES 231516 TO 231556





// ################################################################################





// >>> START OF BLOCK: LINES 232402 TO 232442
// ----------------------------------------
L232402 | 			PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
L232403 | 			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
L232404 | 			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
L232405 | 			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
L232406 | 			PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
L232407 | 			Singleton<CompassManager>.Instance.SetVisible(visible: false);
L232408 | 			List<ItemSlot> allInventorySlots = PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots();
L232409 | 			List<ItemSlot> secondarySlots = new List<ItemSlot>(CustomerSlots);
L232410 | 			if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ItemAmountSelectionTutorialDone") && GameManager.IS_TUTORIAL)
L232411 | 			{
L232412 | 				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ItemAmountSelectionTutorialDone", true.ToString());
L232413 | 				OpenTutorial();
L232414 | 			}
L232415 | 			else
L232416 | 			{
L232417 | 				Player.Local.VisualState.ApplyState("drugdeal", EVisualState.DrugDealing);
L232418 | 			}
L232419 | 			Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
L232420 | 			if (Mode == EMode.Contract)
L232421 | 			{
L232422 | 				DescriptionLabel.text = customer.NPC.FirstName + " is paying <color=#50E65A>" + MoneyManager.FormatAmount(contract.Payment) + "</color> for:";
L232423 | 				DescriptionLabel.enabled = true;
L232424 | 			}
L232425 | 			else
L232426 | 			{
L232427 | 				DescriptionLabel.enabled = false;
L232428 | 			}
L232429 | 			if (Mode == EMode.Sample)
L232430 | 			{
L232431 | 				EDrugType property = customer.GetOrderedDrugTypes()[0];
L232432 | 				string text = ColorUtility.ToHtmlStringRGB(property.GetColor());
L232433 | 				FavouriteDrugLabel.text = customer.NPC.FirstName + "'s favourite drug: <color=#" + text + ">" + property.ToString() + "</color>";
L232434 | 				FavouriteDrugLabel.enabled = true;
L232435 | 				FavouritePropertiesLabel.text = customer.NPC.FirstName + "'s favourite effects:";
L232436 | 				for (int i = 0; i < PropertiesEntries.Length; i++)
L232437 | 				{
L232438 | 					if (customer.CustomerData.PreferredProperties.Count > i)
L232439 | 					{
L232440 | 						PropertiesEntries[i].text = "•  " + customer.CustomerData.PreferredProperties[i].Name;
L232441 | 						PropertiesEntries[i].color = customer.CustomerData.PreferredProperties[i].LabelColor;
L232442 | 						PropertiesEntries[i].enabled = true;

// <<< END OF BLOCK: LINES 232402 TO 232442





// ################################################################################





// >>> START OF BLOCK: LINES 232623 TO 232663
// ----------------------------------------
L232623 | 						PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(itemSlot.ItemInstance);
L232624 | 					}
L232625 | 					itemSlot.ClearStoredInstance();
L232626 | 				}
L232627 | 			}
L232628 | 			OriginalItemLocations.Clear();
L232629 | 			ignoreCustomerChangedEvents = false;
L232630 | 			CustomerItemsChanged();
L232631 | 		}
L232632 | 
L232633 | 		private void CustomerItemsChanged()
L232634 | 		{
L232635 | 			if (!ignoreCustomerChangedEvents)
L232636 | 			{
L232637 | 				UpdateDoneButton();
L232638 | 				UpdateSuccessChance();
L232639 | 				if (Mode == EMode.Offer)
L232640 | 				{
L232641 | 					float customerItemsValue = GetCustomerItemsValue();
L232642 | 					PriceSelector.SetPrice(customerItemsValue);
L232643 | 					FairPriceLabel.text = "Fair price: " + MoneyManager.FormatAmount(customerItemsValue);
L232644 | 				}
L232645 | 			}
L232646 | 		}
L232647 | 
L232648 | 		private void UpdateDoneButton()
L232649 | 		{
L232650 | 			if (GetError(out var err))
L232651 | 			{
L232652 | 				DoneButton.interactable = false;
L232653 | 				ErrorLabel.text = err;
L232654 | 				ErrorLabel.enabled = true;
L232655 | 			}
L232656 | 			else
L232657 | 			{
L232658 | 				DoneButton.interactable = true;
L232659 | 				ErrorLabel.enabled = false;
L232660 | 			}
L232661 | 			if (!ErrorLabel.enabled && GetWarning(out var warning))
L232662 | 			{
L232663 | 				WarningLabel.text = warning;

// <<< END OF BLOCK: LINES 232623 TO 232663





// ################################################################################





// >>> START OF BLOCK: LINES 233431 TO 233523
// ----------------------------------------
L233431 | 
L233432 | 		public bool purchased { get; private set; }
L233433 | 
L233434 | 		private bool purchaseable
L233435 | 		{
L233436 | 			get
L233437 | 			{
L233438 | 				if (RequireLevel)
L233439 | 				{
L233440 | 					return RequiredLevel <= NetworkSingleton<LevelManager>.Instance.GetFullRank();
L233441 | 				}
L233442 | 				return true;
L233443 | 			}
L233444 | 		}
L233445 | 
L233446 | 		private void Awake()
L233447 | 		{
L233448 | 			NameLabel.text = Name;
L233449 | 			if (Price > 0f)
L233450 | 			{
L233451 | 				PriceLabel.text = MoneyManager.FormatAmount(Price);
L233452 | 			}
L233453 | 			else
L233454 | 			{
L233455 | 				PriceLabel.text = "Free";
L233456 | 			}
L233457 | 			UpdatePriceColor();
L233458 | 			LevelLabel.text = RequiredLevel.ToString();
L233459 | 			MainButton.onClick.AddListener(Selected);
L233460 | 			BuyButton.onClick.AddListener(Purchased);
L233461 | 		}
L233462 | 
L233463 | 		private void OnValidate()
L233464 | 		{
L233465 | 			base.gameObject.name = Name;
L233466 | 		}
L233467 | 
L233468 | 		private void FixedUpdate()
L233469 | 		{
L233470 | 			BuyButton.interactable = NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= Price;
L233471 | 		}
L233472 | 
L233473 | 		private void Start()
L233474 | 		{
L233475 | 			UpdateUI();
L233476 | 		}
L233477 | 
L233478 | 		private void Selected()
L233479 | 		{
L233480 | 			SetSelected(_selected: true);
L233481 | 		}
L233482 | 
L233483 | 		private void Purchased()
L233484 | 		{
L233485 | 			if (purchaseable)
L233486 | 			{
L233487 | 				if (onPurchase != null)
L233488 | 				{
L233489 | 					onPurchase.Invoke();
L233490 | 				}
L233491 | 				if (Price > 0f)
L233492 | 				{
L233493 | 					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Character customization", 0f - Price, 1f, string.Empty);
L233494 | 				}
L233495 | 				SetPurchased(_purchased: true);
L233496 | 			}
L233497 | 		}
L233498 | 
L233499 | 		private void UpdatePriceColor()
L233500 | 		{
L233501 | 			if (Price > 0f)
L233502 | 			{
L233503 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= Price)
L233504 | 				{
L233505 | 					PriceLabel.color = (ColorUtility.TryParseHtmlString("#4CBFFF", out var color) ? color : Color.white);
L233506 | 				}
L233507 | 				else
L233508 | 				{
L233509 | 					PriceLabel.color = new Color32(200, 75, 70, byte.MaxValue);
L233510 | 				}
L233511 | 			}
L233512 | 			else
L233513 | 			{
L233514 | 				PriceLabel.color = (ColorUtility.TryParseHtmlString("#4CBFFF", out var color2) ? color2 : Color.white);
L233515 | 			}
L233516 | 		}
L233517 | 
L233518 | 		public void SetSelected(bool _selected)
L233519 | 		{
L233520 | 			selected = _selected;
L233521 | 			SelectionIndicator.gameObject.SetActive(selected);
L233522 | 			NameLabel.rectTransform.offsetMin = new Vector2(selected ? 30f : 10f, NameLabel.rectTransform.offsetMin.y);
L233523 | 			UpdateUI();

// <<< END OF BLOCK: LINES 233431 TO 233523





// ################################################################################





// >>> START OF BLOCK: LINES 234267 TO 234393
// ----------------------------------------
L234267 | 		protected UnityEngine.UI.Button doneButton;
L234268 | 
L234269 | 		private RectTransform activeScreen;
L234270 | 
L234271 | 		public static int[] amounts = new int[6] { 20, 50, 100, 500, 1000, 5000 };
L234272 | 
L234273 | 		private bool depositing = true;
L234274 | 
L234275 | 		private int selectedAmountIndex;
L234276 | 
L234277 | 		private float selectedAmount;
L234278 | 
L234279 | 		public bool isOpen { get; protected set; }
L234280 | 
L234281 | 		private float relevantBalance
L234282 | 		{
L234283 | 			get
L234284 | 			{
L234285 | 				if (!depositing)
L234286 | 				{
L234287 | 					return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance;
L234288 | 				}
L234289 | 				return NetworkSingleton<MoneyManager>.Instance.cashBalance;
L234290 | 			}
L234291 | 		}
L234292 | 
L234293 | 		private static float remainingAllowedDeposit => 10000f - ScheduleOne.Money.ATM.WeeklyDepositSum;
L234294 | 
L234295 | 		private void Awake()
L234296 | 		{
L234297 | 			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
L234298 | 			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
L234299 | 		}
L234300 | 
L234301 | 		private void OnDestroy()
L234302 | 		{
L234303 | 			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
L234304 | 		}
L234305 | 
L234306 | 		protected virtual void Start()
L234307 | 		{
L234308 | 			GameInput.RegisterExitListener(Exit, 2);
L234309 | 			activeScreen = menuScreen;
L234310 | 			canvas.enabled = false;
L234311 | 			for (int i = 0; i < amountButtons.Count; i++)
L234312 | 			{
L234313 | 				int cachedIndex = i;
L234314 | 				amountButtons[i].onClick.AddListener(delegate
L234315 | 				{
L234316 | 					AmountSelected(cachedIndex);
L234317 | 				});
L234318 | 				if (i == amountButtons.Count - 1)
L234319 | 				{
L234320 | 					amountButtons[i].transform.Find("Text").GetComponent<Text>().text = "ALL ()";
L234321 | 				}
L234322 | 				else
L234323 | 				{
L234324 | 					amountButtons[i].transform.Find("Text").GetComponent<Text>().text = MoneyManager.FormatAmount(amounts[i]);
L234325 | 				}
L234326 | 			}
L234327 | 			depositLimitContainer.gameObject.SetActive(value: true);
L234328 | 		}
L234329 | 
L234330 | 		private void PlayerSpawned()
L234331 | 		{
L234332 | 			canvas.worldCamera = PlayerSingleton<PlayerCamera>.Instance.Camera;
L234333 | 		}
L234334 | 
L234335 | 		protected virtual void Update()
L234336 | 		{
L234337 | 			if (!isOpen)
L234338 | 			{
L234339 | 				return;
L234340 | 			}
L234341 | 			onlineBalanceText.text = MoneyManager.FormatAmount(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance);
L234342 | 			cleanCashText.text = MoneyManager.FormatAmount(NetworkSingleton<MoneyManager>.Instance.cashBalance);
L234343 | 			depositLimitText.text = MoneyManager.FormatAmount(ScheduleOne.Money.ATM.WeeklyDepositSum) + " / " + MoneyManager.FormatAmount(10000f);
L234344 | 			if (ScheduleOne.Money.ATM.WeeklyDepositSum >= 10000f)
L234345 | 			{
L234346 | 				depositLimitText.color = new Color32(byte.MaxValue, 75, 75, byte.MaxValue);
L234347 | 			}
L234348 | 			else
L234349 | 			{
L234350 | 				depositLimitText.color = Color.white;
L234351 | 			}
L234352 | 			if (activeScreen == amountSelectorScreen)
L234353 | 			{
L234354 | 				if (depositing)
L234355 | 				{
L234356 | 					amountButtons[amountButtons.Count - 1].transform.Find("Text").GetComponent<Text>().text = "MAX (" + MoneyManager.FormatAmount(Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit)) + ")";
L234357 | 				}
L234358 | 				UpdateAvailableAmounts();
L234359 | 				confirmAmountButton.interactable = relevantBalance > 0f;
L234360 | 				if (depositing)
L234361 | 				{
L234362 | 					if (selectedAmountIndex == amounts.Length)
L234363 | 					{
L234364 | 						confirmButtonText.text = "DEPOSIT ALL";
L234365 | 					}
L234366 | 					else
L234367 | 					{
L234368 | 						confirmButtonText.text = "DEPOSIT " + MoneyManager.FormatAmount(selectedAmount);
L234369 | 					}
L234370 | 				}
L234371 | 				else
L234372 | 				{
L234373 | 					confirmButtonText.text = "WITHDRAW " + MoneyManager.FormatAmount(selectedAmount);
L234374 | 				}
L234375 | 				if (relevantBalance < GetAmountFromIndex(selectedAmountIndex, depositing))
L234376 | 				{
L234377 | 					DefaultAmountSelection();
L234378 | 				}
L234379 | 			}
L234380 | 			if (activeScreen == menuScreen)
L234381 | 			{
L234382 | 				menu_DepositButton.interactable = ScheduleOne.Money.ATM.WeeklyDepositSum < 10000f;
L234383 | 			}
L234384 | 			if (activeScreen == processingScreen)
L234385 | 			{
L234386 | 				processingScreenIndicator.localEulerAngles = new Vector3(0f, 0f, processingScreenIndicator.localEulerAngles.z - Time.deltaTime * 360f);
L234387 | 			}
L234388 | 		}
L234389 | 
L234390 | 		protected virtual void LateUpdate()
L234391 | 		{
L234392 | 			if (isOpen && activeScreen == amountSelectorScreen)
L234393 | 			{

// <<< END OF BLOCK: LINES 234267 TO 234393





// ################################################################################





// >>> START OF BLOCK: LINES 234479 TO 234610
// ----------------------------------------
L234479 | 				AmountSelected(amountButtons.Count - 1);
L234480 | 				return;
L234481 | 			}
L234482 | 			AmountSelected(-1);
L234483 | 			for (int i = 0; i < amountButtons.Count; i++)
L234484 | 			{
L234485 | 			}
L234486 | 		}
L234487 | 
L234488 | 		public void DepositButtonPressed()
L234489 | 		{
L234490 | 			amountSelectorTitle.text = "Select amount to deposit";
L234491 | 			depositing = true;
L234492 | 			SetActiveScreen(amountSelectorScreen);
L234493 | 		}
L234494 | 
L234495 | 		public void WithdrawButtonPressed()
L234496 | 		{
L234497 | 			amountSelectorTitle.text = "Select amount to withdraw";
L234498 | 			depositing = false;
L234499 | 			amountButtons[amountButtons.Count - 1].transform.Find("Text").GetComponent<Text>().text = MoneyManager.FormatAmount(amounts[amounts.Length - 1]);
L234500 | 			SetActiveScreen(amountSelectorScreen);
L234501 | 		}
L234502 | 
L234503 | 		public void CancelAmountSelection()
L234504 | 		{
L234505 | 			SetActiveScreen(menuScreen);
L234506 | 		}
L234507 | 
L234508 | 		public void AmountSelected(int amountIndex)
L234509 | 		{
L234510 | 			selectedAmountIndex = amountIndex;
L234511 | 			SetSelectedAmount(GetAmountFromIndex(amountIndex, depositing));
L234512 | 		}
L234513 | 
L234514 | 		private void SetSelectedAmount(float amount)
L234515 | 		{
L234516 | 			float num = 0f;
L234517 | 			num = ((!depositing) ? NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance : Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit));
L234518 | 			selectedAmount = Mathf.Clamp(amount, 0f, num);
L234519 | 			amountLabelText.text = MoneyManager.FormatAmount(selectedAmount);
L234520 | 		}
L234521 | 
L234522 | 		public static float GetAmountFromIndex(int index, bool depositing)
L234523 | 		{
L234524 | 			if (index == -1 || index >= amounts.Length)
L234525 | 			{
L234526 | 				return 0f;
L234527 | 			}
L234528 | 			if (depositing && index == amounts.Length - 1)
L234529 | 			{
L234530 | 				return Mathf.Min(NetworkSingleton<MoneyManager>.Instance.cashBalance, remainingAllowedDeposit);
L234531 | 			}
L234532 | 			return amounts[index];
L234533 | 		}
L234534 | 
L234535 | 		private void UpdateAvailableAmounts()
L234536 | 		{
L234537 | 			for (int i = 0; i < amounts.Length; i++)
L234538 | 			{
L234539 | 				if (depositing && i == amounts.Length - 1)
L234540 | 				{
L234541 | 					amountButtons[amountButtons.Count - 1].interactable = relevantBalance > 0f && remainingAllowedDeposit > 0f;
L234542 | 					break;
L234543 | 				}
L234544 | 				if (depositing)
L234545 | 				{
L234546 | 					amountButtons[i].interactable = relevantBalance >= (float)amounts[i] && ScheduleOne.Money.ATM.WeeklyDepositSum + (float)amounts[i] <= 10000f;
L234547 | 				}
L234548 | 				else
L234549 | 				{
L234550 | 					amountButtons[i].interactable = relevantBalance >= (float)amounts[i];
L234551 | 				}
L234552 | 			}
L234553 | 		}
L234554 | 
L234555 | 		public void AmountConfirmed()
L234556 | 		{
L234557 | 			StartCoroutine(ProcessTransaction(selectedAmount, depositing));
L234558 | 		}
L234559 | 
L234560 | 		public void ChangeAmount(float amount)
L234561 | 		{
L234562 | 			selectedAmountIndex = -1;
L234563 | 			SetSelectedAmount(selectedAmount + amount);
L234564 | 		}
L234565 | 
L234566 | 		protected IEnumerator ProcessTransaction(float amount, bool depositing)
L234567 | 		{
L234568 | 			SetActiveScreen(processingScreen);
L234569 | 			yield return new WaitForSeconds(1f);
L234570 | 			CompleteSound.Play();
L234571 | 			if (depositing)
L234572 | 			{
L234573 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= amount)
L234574 | 				{
L234575 | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - amount);
L234576 | 					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Cash Deposit", amount, 1f, string.Empty);
L234577 | 					ScheduleOne.Money.ATM.WeeklyDepositSum += amount;
L234578 | 					successScreenSubtitle.text = "You have deposited " + MoneyManager.FormatAmount(amount);
L234579 | 					SetActiveScreen(successScreen);
L234580 | 				}
L234581 | 				else
L234582 | 				{
L234583 | 					SetActiveScreen(menuScreen);
L234584 | 				}
L234585 | 			}
L234586 | 			else if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= amount)
L234587 | 			{
L234588 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(amount);
L234589 | 				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Cash Withdrawal", 0f - amount, 1f, string.Empty);
L234590 | 				successScreenSubtitle.text = "You have withdrawn " + MoneyManager.FormatAmount(amount);
L234591 | 				SetActiveScreen(successScreen);
L234592 | 			}
L234593 | 			else
L234594 | 			{
L234595 | 				SetActiveScreen(menuScreen);
L234596 | 			}
L234597 | 		}
L234598 | 
L234599 | 		public void DoneButtonPressed()
L234600 | 		{
L234601 | 			SetIsOpen(o: false);
L234602 | 		}
L234603 | 
L234604 | 		public void ReturnToMenuButtonPressed()
L234605 | 		{
L234606 | 			SetActiveScreen(menuScreen);
L234607 | 		}
L234608 | 	}
L234609 | }
L234610 | namespace ScheduleOne.UI.Shop

// <<< END OF BLOCK: LINES 234479 TO 234610





// ################################################################################





// >>> START OF BLOCK: LINES 234975 TO 235119
// ----------------------------------------
L234975 | 				{
L234976 | 					CartIcon.transform.localScale = Vector3.Lerp(startScale, endScale, i / lerpTime);
L234977 | 					yield return new WaitForEndOfFrame();
L234978 | 				}
L234979 | 				for (float i = 0f; i < lerpTime; i += Time.deltaTime)
L234980 | 				{
L234981 | 					CartIcon.transform.localScale = Vector3.Lerp(endScale, startScale, i / lerpTime);
L234982 | 					yield return new WaitForEndOfFrame();
L234983 | 				}
L234984 | 				CartIcon.transform.localScale = startScale;
L234985 | 				cartIconBop = null;
L234986 | 			}
L234987 | 		}
L234988 | 
L234989 | 		public bool CanPlayerAffordCart()
L234990 | 		{
L234991 | 			float priceSum = GetPriceSum();
L234992 | 			switch (Shop.PaymentType)
L234993 | 			{
L234994 | 			case ShopInterface.EPaymentType.Cash:
L234995 | 				return NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum;
L234996 | 			case ShopInterface.EPaymentType.Online:
L234997 | 				return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum;
L234998 | 			case ShopInterface.EPaymentType.PreferCash:
L234999 | 				if (!(NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum))
L235000 | 				{
L235001 | 					return NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum;
L235002 | 				}
L235003 | 				return true;
L235004 | 			case ShopInterface.EPaymentType.PreferOnline:
L235005 | 				if (!(NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= priceSum))
L235006 | 				{
L235007 | 					return NetworkSingleton<MoneyManager>.Instance.cashBalance >= priceSum;
L235008 | 				}
L235009 | 				return true;
L235010 | 			default:
L235011 | 				return false;
L235012 | 			}
L235013 | 		}
L235014 | 
L235015 | 		public void Buy()
L235016 | 		{
L235017 | 			if (!CanCheckout(out var _))
L235018 | 			{
L235019 | 				return;
L235020 | 			}
L235021 | 			foreach (KeyValuePair<ShopListing, int> item in cartDictionary)
L235022 | 			{
L235023 | 				ShopListing key = item.Key;
L235024 | 				int value = item.Value;
L235025 | 				if (!key.IsUnlimitedStock)
L235026 | 				{
L235027 | 					key.RemoveStock(value);
L235028 | 				}
L235029 | 			}
L235030 | 			Shop.HandoverItems();
L235031 | 			switch (Shop.PaymentType)
L235032 | 			{
L235033 | 			case ShopInterface.EPaymentType.Cash:
L235034 | 				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - GetPriceSum());
L235035 | 				break;
L235036 | 			case ShopInterface.EPaymentType.Online:
L235037 | 				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - GetPriceSum(), 1f, string.Empty);
L235038 | 				break;
L235039 | 			case ShopInterface.EPaymentType.PreferCash:
L235040 | 				if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= GetPriceSum())
L235041 | 				{
L235042 | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - GetPriceSum());
L235043 | 				}
L235044 | 				else
L235045 | 				{
L235046 | 					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - GetPriceSum(), 1f, string.Empty);
L235047 | 				}
L235048 | 				break;
L235049 | 			case ShopInterface.EPaymentType.PreferOnline:
L235050 | 				if (NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance >= GetPriceSum())
L235051 | 				{
L235052 | 					NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Purchase from " + Shop.ShopName, 0f - GetPriceSum(), 1f, string.Empty);
L235053 | 				}
L235054 | 				else
L235055 | 				{
L235056 | 					NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - GetPriceSum());
L235057 | 				}
L235058 | 				break;
L235059 | 			}
L235060 | 			ClearCart();
L235061 | 			Shop.CheckoutSound.Play();
L235062 | 			Shop.SetIsOpen(isOpen: false);
L235063 | 			if (Shop.onOrderCompleted != null)
L235064 | 			{
L235065 | 				Shop.onOrderCompleted.Invoke();
L235066 | 			}
L235067 | 		}
L235068 | 
L235069 | 		private void UpdateEntries()
L235070 | 		{
L235071 | 			List<ShopListing> list = cartDictionary.Keys.ToList();
L235072 | 			for (int i = 0; i < list.Count; i++)
L235073 | 			{
L235074 | 				CartEntry cartEntry = GetEntry(list[i]);
L235075 | 				if (cartEntry == null)
L235076 | 				{
L235077 | 					cartEntry = UnityEngine.Object.Instantiate(EntryPrefab, CartEntryContainer);
L235078 | 					cartEntry.Initialize(this, list[i], cartDictionary[list[i]]);
L235079 | 					cartEntries.Add(cartEntry);
L235080 | 				}
L235081 | 				if (cartEntry.Quantity != cartDictionary[list[i]])
L235082 | 				{
L235083 | 					cartEntry.SetQuantity(cartDictionary[list[i]]);
L235084 | 				}
L235085 | 			}
L235086 | 			for (int j = 0; j < cartEntries.Count; j++)
L235087 | 			{
L235088 | 				if (!cartDictionary.ContainsKey(cartEntries[j].Listing))
L235089 | 				{
L235090 | 					UnityEngine.Object.Destroy(cartEntries[j].gameObject);
L235091 | 					cartEntries.RemoveAt(j);
L235092 | 					j--;
L235093 | 				}
L235094 | 			}
L235095 | 		}
L235096 | 
L235097 | 		private void UpdateTotal()
L235098 | 		{
L235099 | 			TotalText.text = "Total: <color=#" + ColorUtility.ToHtmlStringRGBA(ListingUI.PriceLabelColor_Normal) + ">" + MoneyManager.FormatAmount(GetPriceSum()) + "</color>";
L235100 | 		}
L235101 | 
L235102 | 		private void UpdateProblem()
L235103 | 		{
L235104 | 			string reason;
L235105 | 			bool flag = CanCheckout(out reason);
L235106 | 			BuyButton.interactable = flag && cartDictionary.Count > 0;
L235107 | 			if (flag)
L235108 | 			{
L235109 | 				ProblemText.enabled = false;
L235110 | 			}
L235111 | 			else
L235112 | 			{
L235113 | 				ProblemText.text = reason;
L235114 | 				ProblemText.enabled = true;
L235115 | 			}
L235116 | 			if (GetWarning(out var warning) && !ProblemText.enabled)
L235117 | 			{
L235118 | 				WarningText.text = warning;
L235119 | 				WarningText.enabled = true;

// <<< END OF BLOCK: LINES 234975 TO 235119





// ################################################################################





// >>> START OF BLOCK: LINES 235281 TO 235321
// ----------------------------------------
L235281 | 				ChangeAmount(-999);
L235282 | 			});
L235283 | 			UpdateTitle();
L235284 | 			UpdatePrice();
L235285 | 		}
L235286 | 
L235287 | 		public void SetQuantity(int quantity)
L235288 | 		{
L235289 | 			Quantity = quantity;
L235290 | 			UpdateTitle();
L235291 | 			UpdatePrice();
L235292 | 		}
L235293 | 
L235294 | 		protected virtual void UpdateTitle()
L235295 | 		{
L235296 | 			NameLabel.text = Quantity + "x " + Listing.Item.Name;
L235297 | 		}
L235298 | 
L235299 | 		private void UpdatePrice()
L235300 | 		{
L235301 | 			PriceLabel.text = MoneyManager.FormatAmount((float)Quantity * Listing.Price);
L235302 | 		}
L235303 | 
L235304 | 		private void ChangeAmount(int change)
L235305 | 		{
L235306 | 			if (change > 0)
L235307 | 			{
L235308 | 				Cart.AddItem(Listing, change);
L235309 | 			}
L235310 | 			else if (change < 0)
L235311 | 			{
L235312 | 				Cart.RemoveItem(Listing, -change);
L235313 | 			}
L235314 | 		}
L235315 | 	}
L235316 | 	public class CartEntry_Clothing : CartEntry
L235317 | 	{
L235318 | 		protected override void UpdateTitle()
L235319 | 		{
L235320 | 			base.UpdateTitle();
L235321 | 			if ((base.Listing.Item as ClothingDefinition).Colorable)

// <<< END OF BLOCK: LINES 235281 TO 235321





// ################################################################################





// >>> START OF BLOCK: LINES 235608 TO 235648
// ----------------------------------------
L235608 | 			}
L235609 | 		}
L235610 | 
L235611 | 		private void HoverEnd()
L235612 | 		{
L235613 | 			if (hoverEnd != null)
L235614 | 			{
L235615 | 				hoverEnd();
L235616 | 			}
L235617 | 		}
L235618 | 
L235619 | 		private void StockChanged()
L235620 | 		{
L235621 | 			UpdateButtons();
L235622 | 			UpdatePrice();
L235623 | 			UpdateStock();
L235624 | 		}
L235625 | 
L235626 | 		private void UpdatePrice()
L235627 | 		{
L235628 | 			PriceLabel.text = MoneyManager.FormatAmount(Listing.Price);
L235629 | 			PriceLabel.color = PriceLabelColor_Normal;
L235630 | 		}
L235631 | 
L235632 | 		private void UpdateStock()
L235633 | 		{
L235634 | 			if (StockLabel == null)
L235635 | 			{
L235636 | 				return;
L235637 | 			}
L235638 | 			if (Listing.IsUnlimitedStock)
L235639 | 			{
L235640 | 				StockLabel.enabled = false;
L235641 | 				return;
L235642 | 			}
L235643 | 			int currentStockMinusCart = Listing.CurrentStockMinusCart;
L235644 | 			if (Listing.TieStockToNumberVariable)
L235645 | 			{
L235646 | 				StockLabel.text = currentStockMinusCart.ToString();
L235647 | 			}
L235648 | 			else

// <<< END OF BLOCK: LINES 235608 TO 235648





// ################################################################################





// >>> START OF BLOCK: LINES 236755 TO 236795
// ----------------------------------------
L236755 | 
L236756 | 		public TextMeshProUGUI AmountLabel;
L236757 | 
L236758 | 		public override void Setup(ItemInstance item)
L236759 | 		{
L236760 | 			cashInstance = item as CashInstance;
L236761 | 			base.Setup(item);
L236762 | 		}
L236763 | 
L236764 | 		public override void UpdateUI()
L236765 | 		{
L236766 | 			base.UpdateUI();
L236767 | 			if (!Destroyed)
L236768 | 			{
L236769 | 				SetDisplayedBalance(cashInstance.Balance);
L236770 | 			}
L236771 | 		}
L236772 | 
L236773 | 		public void SetDisplayedBalance(float balance)
L236774 | 		{
L236775 | 			AmountLabel.text = MoneyManager.FormatAmount(balance);
L236776 | 		}
L236777 | 	}
L236778 | 	public class ClothingItemUI : ItemUI
L236779 | 	{
L236780 | 		public Image ClothingTypeIcon;
L236781 | 
L236782 | 		public override void UpdateUI()
L236783 | 		{
L236784 | 			base.UpdateUI();
L236785 | 			ClothingInstance clothingInstance = itemInstance as ClothingInstance;
L236786 | 			if (itemInstance != null && (itemInstance.Definition as ClothingDefinition).Colorable)
L236787 | 			{
L236788 | 				IconImg.color = clothingInstance.Color.GetActualColor();
L236789 | 			}
L236790 | 			else
L236791 | 			{
L236792 | 				IconImg.color = Color.white;
L236793 | 			}
L236794 | 			if (itemInstance != null)
L236795 | 			{

// <<< END OF BLOCK: LINES 236755 TO 236795





// ################################################################################





// >>> START OF BLOCK: LINES 237794 TO 237834
// ----------------------------------------
L237794 | 		}
L237795 | 
L237796 | 		protected virtual void LateUpdate()
L237797 | 		{
L237798 | 			if (DraggingEnabled && draggedSlot != null)
L237799 | 			{
L237800 | 				tempIcon.position = new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y) - mouseOffset;
L237801 | 				if (customDragAmount)
L237802 | 				{
L237803 | 					ItemQuantityPrompt.position = tempIcon.position + new Vector3(0f, tempIcon.rect.height * 0.5f + 25f, 0f);
L237804 | 				}
L237805 | 			}
L237806 | 			UpdateCashDragSelectorUI();
L237807 | 		}
L237808 | 
L237809 | 		private void UpdateCashDragSelectorUI()
L237810 | 		{
L237811 | 			if (draggedSlot != null && draggedSlot.assignedSlot != null && draggedSlot.assignedSlot.ItemInstance != null && draggedSlot.assignedSlot.ItemInstance is CashInstance && customDragAmount)
L237812 | 			{
L237813 | 				_ = draggedSlot.assignedSlot.ItemInstance;
L237814 | 				tempIcon.Find("Balance").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(draggedCashAmount);
L237815 | 				CashDragAmountContainer.position = tempIcon.position + new Vector3(0f, tempIcon.rect.height * 0.5f + 15f, 0f);
L237816 | 				CashDragAmountContainer.gameObject.SetActive(value: true);
L237817 | 			}
L237818 | 			else
L237819 | 			{
L237820 | 				CashDragAmountContainer.gameObject.SetActive(value: false);
L237821 | 			}
L237822 | 		}
L237823 | 
L237824 | 		private void UpdateCashDragAmount(CashInstance instance)
L237825 | 		{
L237826 | 			float[] array = new float[3] { 50f, 10f, 1f };
L237827 | 			float[] array2 = new float[3] { 100f, 10f, 1f };
L237828 | 			float num = 0f;
L237829 | 			if (GameInput.MouseScrollDelta > 0f)
L237830 | 			{
L237831 | 				for (int i = 0; i < array.Length; i++)
L237832 | 				{
L237833 | 					if (draggedCashAmount >= array2[i])
L237834 | 					{

// <<< END OF BLOCK: LINES 237794 TO 237834





// ################################################################################





// >>> START OF BLOCK: LINES 238098 TO 238138
// ----------------------------------------
L238098 | 						draggedSlot.assignedSlot.ReplicateStoredInstance();
L238099 | 					}
L238100 | 				}
L238101 | 				if (onItemMoved != null)
L238102 | 				{
L238103 | 					onItemMoved.Invoke();
L238104 | 				}
L238105 | 				draggedSlot = null;
L238106 | 			}
L238107 | 			else
L238108 | 			{
L238109 | 				if (onDragStart != null)
L238110 | 				{
L238111 | 					onDragStart.Invoke();
L238112 | 				}
L238113 | 				if (draggedSlot.assignedSlot != PlayerSingleton<PlayerInventory>.Instance.cashSlot)
L238114 | 				{
L238115 | 					CashSlotHintAnim.Play();
L238116 | 				}
L238117 | 				tempIcon = draggedSlot.DuplicateIcon(Singleton<HUD>.Instance.transform, draggedAmount);
L238118 | 				tempIcon.Find("Balance").GetComponent<TextMeshProUGUI>().text = MoneyManager.FormatAmount(draggedCashAmount);
L238119 | 				draggedSlot.IsBeingDragged = true;
L238120 | 				if (draggedCashAmount >= cashInstance.Balance)
L238121 | 				{
L238122 | 					draggedSlot.SetVisible(shown: false);
L238123 | 				}
L238124 | 				else
L238125 | 				{
L238126 | 					(draggedSlot.ItemUI as ItemUI_Cash).SetDisplayedBalance(cashInstance.Balance - draggedCashAmount);
L238127 | 				}
L238128 | 			}
L238129 | 		}
L238130 | 
L238131 | 		private void EndDrag()
L238132 | 		{
L238133 | 			if (isDraggingCash)
L238134 | 			{
L238135 | 				EndCashDrag();
L238136 | 				return;
L238137 | 			}
L238138 | 			if (CanDragFromSlot(draggedSlot) && HoveredSlot != null && HoveredSlot != draggedSlot && HoveredSlot.assignedSlot != null && !HoveredSlot.assignedSlot.IsLocked && !HoveredSlot.assignedSlot.IsAddLocked && HoveredSlot.assignedSlot.DoesItemMatchHardFilters(draggedSlot.assignedSlot.ItemInstance))

// <<< END OF BLOCK: LINES 238098 TO 238138





// ################################################################################





// >>> START OF BLOCK: LINES 238597 TO 238651
// ----------------------------------------
L238597 | 			set
L238598 | 			{
L238599 | 				if (value || !base.IsServerInitialized)
L238600 | 				{
L238601 | 					Value = value;
L238602 | 				}
L238603 | 				if (UnityEngine.Application.isPlaying)
L238604 | 				{
L238605 | 					syncVar___Value.SetValue(value, value);
L238606 | 				}
L238607 | 			}
L238608 | 		}
L238609 | 
L238610 | 		private void Start()
L238611 | 		{
L238612 | 			UpdateCashStackVisuals();
L238613 | 		}
L238614 | 
L238615 | 		protected override void Hovered()
L238616 | 		{
L238617 | 			IntObj.SetMessage("Pick up " + MoneyManager.FormatAmount(SyncAccessor_Value));
L238618 | 			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
L238619 | 		}
L238620 | 
L238621 | 		protected override bool CanPickup()
L238622 | 		{
L238623 | 			return true;
L238624 | 		}
L238625 | 
L238626 | 		protected override void Pickup()
L238627 | 		{
L238628 | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(SyncAccessor_Value);
L238629 | 			if (PlayCashPickupSound)
L238630 | 			{
L238631 | 				NetworkSingleton<MoneyManager>.Instance.PlayCashSound();
L238632 | 			}
L238633 | 			base.Pickup();
L238634 | 		}
L238635 | 
L238636 | 		private void ValueChanged(float oldValue, float newValue, bool asServer)
L238637 | 		{
L238638 | 			UpdateCashStackVisuals();
L238639 | 		}
L238640 | 
L238641 | 		private void UpdateCashStackVisuals()
L238642 | 		{
L238643 | 			if (CashStackVisuals != null)
L238644 | 			{
L238645 | 				CashStackVisuals.ShowAmount(SyncAccessor_Value);
L238646 | 			}
L238647 | 		}
L238648 | 
L238649 | 		public override void NetworkInitialize___Early()
L238650 | 		{
L238651 | 			if (!NetworkInitialize___EarlyScheduleOne.ItemFramework.CashPickupAssembly-CSharp.dll_Excuted)

// <<< END OF BLOCK: LINES 238597 TO 238651





// ################################################################################





// >>> START OF BLOCK: LINES 247647 TO 247735
// ----------------------------------------
L247647 | 			PhysicsDamageable damageable = Damageable;
L247648 | 			damageable.onImpacted = (Action<Impact>)Delegate.Combine(damageable.onImpacted, new Action<Impact>(Impacted));
L247649 | 			GUID = new Guid(BakedGUID);
L247650 | 			GUIDManager.RegisterObject(this);
L247651 | 		}
L247652 | 	}
L247653 | 	public class CashSlot : HotbarSlot
L247654 | 	{
L247655 | 		public const float MAX_CASH_PER_SLOT = 1000f;
L247656 | 
L247657 | 		public override void ClearStoredInstance(bool _internal = false)
L247658 | 		{
L247659 | 			(base.ItemInstance as CashInstance).SetBalance(0f, blockClear: true);
L247660 | 		}
L247661 | 
L247662 | 		public override bool CanSlotAcceptCash()
L247663 | 		{
L247664 | 			return true;
L247665 | 		}
L247666 | 	}
L247667 | 	public class MoneyManager : NetworkSingleton<MoneyManager>, IBaseSaveable, ISaveable
L247668 | 	{
L247669 | 		public class FloatContainer
L247670 | 		{
L247671 | 			public float value { get; private set; }
L247672 | 
L247673 | 			public void ChangeValue(float value)
L247674 | 			{
L247675 | 				this.value += value;
L247676 | 			}
L247677 | 		}
L247678 | 
L247679 | 		public const string MONEY_TEXT_COLOR = "#54E717";
L247680 | 
L247681 | 		public const string MONEY_TEXT_COLOR_DARKER = "#46CB4F";
L247682 | 
L247683 | 		public const string ONLINE_BALANCE_COLOR = "#4CBFFF";
L247684 | 
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

// <<< END OF BLOCK: LINES 247647 TO 247735





// ################################################################################





// >>> START OF BLOCK: LINES 247777 TO 247817
// ----------------------------------------
L247777 | 		}
L247778 | 
L247779 | 		public static string ApplyMoneyTextColor(string text)
L247780 | 		{
L247781 | 			return "<color=#54E717>" + text + "</color>";
L247782 | 		}
L247783 | 
L247784 | 		public static string ApplyMoneyTextColorDarker(string text)
L247785 | 		{
L247786 | 			return "<color=#46CB4F>" + text + "</color>";
L247787 | 		}
L247788 | 
L247789 | 		public static string ApplyOnlineBalanceColor(string text)
L247790 | 		{
L247791 | 			return "<color=#4CBFFF>" + text + "</color>";
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

// <<< END OF BLOCK: LINES 247777 TO 247817





// ################################################################################





// >>> START OF BLOCK: LINES 248007 TO 248064
// ----------------------------------------
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
L248042 | 			if (!NetworkInitialize__LateScheduleOne.Money.MoneyManagerAssembly-CSharp.dll_Excuted)
L248043 | 			{
L248044 | 				NetworkInitialize__LateScheduleOne.Money.MoneyManagerAssembly-CSharp.dll_Excuted = true;
L248045 | 				base.NetworkInitialize__Late();
L248046 | 				syncVar___lifetimeEarnings.SetRegistered();
L248047 | 				syncVar___onlineBalance.SetRegistered();
L248048 | 			}
L248049 | 		}
L248050 | 
L248051 | 		public override void NetworkInitializeIfDisabled()
L248052 | 		{
L248053 | 			NetworkInitialize___Early();
L248054 | 			NetworkInitialize__Late();
L248055 | 		}
L248056 | 
L248057 | 		private void RpcWriter___Server_CreateOnlineTransaction_1419830531(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
L248058 | 		{
L248059 | 			if (!base.IsClientInitialized)
L248060 | 			{
L248061 | 				NetworkManager networkManager = base.NetworkManager;
L248062 | 				if ((object)networkManager == null)
L248063 | 				{
L248064 | 					networkManager = InstanceFinder.NetworkManager;

// <<< END OF BLOCK: LINES 248007 TO 248064





// ################################################################################





// >>> START OF BLOCK: LINES 248196 TO 248267
// ----------------------------------------
L248196 | 				SendServerRpc(2u, writer, channel, DataOrderType.Default);
L248197 | 				writer.Store();
L248198 | 			}
L248199 | 		}
L248200 | 
L248201 | 		public void RpcLogic___ChangeLifetimeEarnings_431000436(float change)
L248202 | 		{
L248203 | 			this.sync___set_value_lifetimeEarnings(Mathf.Clamp(SyncAccessor_lifetimeEarnings + change, 0f, float.MaxValue), asServer: true);
L248204 | 			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("LifetimeEarnings", lifetimeEarnings.ToString());
L248205 | 		}
L248206 | 
L248207 | 		private void RpcReader___Server_ChangeLifetimeEarnings_431000436(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
L248208 | 		{
L248209 | 			float change = PooledReader0.ReadSingle();
L248210 | 			if (base.IsServerInitialized)
L248211 | 			{
L248212 | 				RpcLogic___ChangeLifetimeEarnings_431000436(change);
L248213 | 			}
L248214 | 		}
L248215 | 
L248216 | 		public virtual bool ReadSyncVar___ScheduleOne.Money.MoneyManager(PooledReader PooledReader0, uint UInt321, bool Boolean2)
L248217 | 		{
L248218 | 			switch (UInt321)
L248219 | 			{
L248220 | 			case 1u:
L248221 | 			{
L248222 | 				if (PooledReader0 == null)
L248223 | 				{
L248224 | 					this.sync___set_value_lifetimeEarnings(syncVar___lifetimeEarnings.GetValue(calledByUser: true), asServer: true);
L248225 | 					return true;
L248226 | 				}
L248227 | 				float value2 = PooledReader0.ReadSingle();
L248228 | 				this.sync___set_value_lifetimeEarnings(value2, Boolean2);
L248229 | 				return true;
L248230 | 			}
L248231 | 			case 0u:
L248232 | 			{
L248233 | 				if (PooledReader0 == null)
L248234 | 				{
L248235 | 					this.sync___set_value_onlineBalance(syncVar___onlineBalance.GetValue(calledByUser: true), asServer: true);
L248236 | 					return true;
L248237 | 				}
L248238 | 				float value = PooledReader0.ReadSingle();
L248239 | 				this.sync___set_value_onlineBalance(value, Boolean2);
L248240 | 				return true;
L248241 | 			}
L248242 | 			default:
L248243 | 				return false;
L248244 | 			}
L248245 | 		}
L248246 | 
L248247 | 		protected virtual void Awake_UserLogic_ScheduleOne.Money.MoneyManager_Assembly-CSharp.dll()
L248248 | 		{
L248249 | 			base.Awake();
L248250 | 			InitializeSaveable();
L248251 | 		}
L248252 | 	}
L248253 | 	public class Transaction
L248254 | 	{
L248255 | 		public string transaction_Name = string.Empty;
L248256 | 
L248257 | 		public float unit_Amount;
L248258 | 
L248259 | 		public float quantity = 1f;
L248260 | 
L248261 | 		public string transaction_Note = string.Empty;
L248262 | 
L248263 | 		public float total_Amount => unit_Amount * quantity;
L248264 | 
L248265 | 		public Transaction(string _transaction_Name, float _unit_Amount, float _quantity, string _transaction_Note)
L248266 | 		{
L248267 | 			transaction_Name = _transaction_Name;

// <<< END OF BLOCK: LINES 248196 TO 248267





// ################################################################################





// >>> START OF BLOCK: LINES 248462 TO 248511
// ----------------------------------------
L248462 | 					ButtonLight.isOn = true;
L248463 | 					PressBeginInstruction.gameObject.SetActive(value: true);
L248464 | 				}
L248465 | 				else
L248466 | 				{
L248467 | 					ButtonIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
L248468 | 					ButtonLight.isOn = false;
L248469 | 					InsertTrashInstruction.gameObject.SetActive(value: true);
L248470 | 				}
L248471 | 			}
L248472 | 		}
L248473 | 
L248474 | 		public void HandleInteracted()
L248475 | 		{
L248476 | 			SendState(EState.HatchOpen);
L248477 | 		}
L248478 | 
L248479 | 		public void ButtonInteracted()
L248480 | 		{
L248481 | 			ProcessingLabel.text = "Processing...";
L248482 | 			ValueLabel.text = MoneyManager.FormatAmount(0f);
L248483 | 			PressSound.Play();
L248484 | 			SendState(EState.Processing);
L248485 | 			StartCoroutine(Process(startedByLocalPlayer: true));
L248486 | 		}
L248487 | 
L248488 | 		public void CashInteracted()
L248489 | 		{
L248490 | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(cashValue);
L248491 | 			NetworkSingleton<MoneyManager>.Instance.ChangeLifetimeEarnings(cashValue);
L248492 | 			SendState(EState.HatchClosed);
L248493 | 			BankNote.gameObject.SetActive(value: false);
L248494 | 			cashValue = 0f;
L248495 | 			SendCashCollected();
L248496 | 		}
L248497 | 
L248498 | 		[ServerRpc(RequireOwnership = false)]
L248499 | 		private void SendCashCollected()
L248500 | 		{
L248501 | 			RpcWriter___Server_SendCashCollected_2166136261();
L248502 | 		}
L248503 | 
L248504 | 		[ObserversRpc(RunLocally = true)]
L248505 | 		private void CashCollected()
L248506 | 		{
L248507 | 			RpcWriter___Observers_CashCollected_2166136261();
L248508 | 			RpcLogic___CashCollected_2166136261();
L248509 | 		}
L248510 | 
L248511 | 		[ObserversRpc(RunLocally = true)]

// <<< END OF BLOCK: LINES 248462 TO 248511





// ################################################################################





// >>> START OF BLOCK: LINES 248552 TO 248600
// ----------------------------------------
L248552 | 					}
L248553 | 				}
L248554 | 				else
L248555 | 				{
L248556 | 					value += (float)trashItem.SellValue;
L248557 | 				}
L248558 | 				if (InstanceFinder.IsServer)
L248559 | 				{
L248560 | 					trashItem.DestroyTrash();
L248561 | 				}
L248562 | 			}
L248563 | 			if (cashValue <= 0f)
L248564 | 			{
L248565 | 				SetCashValue(value);
L248566 | 			}
L248567 | 			float lerpTime = 1.5f;
L248568 | 			for (float i2 = 0f; i2 < lerpTime; i2 += Time.deltaTime)
L248569 | 			{
L248570 | 				float t = i2 / lerpTime;
L248571 | 				float amount = Mathf.Lerp(0f, cashValue, t);
L248572 | 				ValueLabel.text = MoneyManager.FormatAmount(amount, showDecimals: true);
L248573 | 				yield return new WaitForEndOfFrame();
L248574 | 			}
L248575 | 			if (onStop != null)
L248576 | 			{
L248577 | 				onStop.Invoke();
L248578 | 			}
L248579 | 			ProcessingLabel.text = "Thank you";
L248580 | 			ValueLabel.text = MoneyManager.FormatAmount(value);
L248581 | 			DoneSound.Play();
L248582 | 			yield return new WaitForSeconds(0.3f);
L248583 | 			CashEjectSound.Play();
L248584 | 			CashAnim.Play();
L248585 | 			yield return new WaitForSeconds(0.25f);
L248586 | 			if (InstanceFinder.IsServer)
L248587 | 			{
L248588 | 				EnableCash();
L248589 | 			}
L248590 | 		}
L248591 | 
L248592 | 		[ServerRpc(RequireOwnership = false, RunLocally = true)]
L248593 | 		public void SendState(EState state)
L248594 | 		{
L248595 | 			RpcWriter___Server_SendState_3569965459(state);
L248596 | 			RpcLogic___SendState_3569965459(state);
L248597 | 		}
L248598 | 
L248599 | 		[ObserversRpc(RunLocally = true)]
L248600 | 		[TargetRpc]

// <<< END OF BLOCK: LINES 248552 TO 248600





// ################################################################################





// >>> START OF BLOCK: LINES 249170 TO 249232
// ----------------------------------------
L249170 | 			if (IsBroken)
L249171 | 			{
L249172 | 				DaysUntilRepair--;
L249173 | 				if (DaysUntilRepair <= 0)
L249174 | 				{
L249175 | 					Repair();
L249176 | 				}
L249177 | 			}
L249178 | 		}
L249179 | 
L249180 | 		public void Hovered()
L249181 | 		{
L249182 | 			if (purchaseInProgress)
L249183 | 			{
L249184 | 				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
L249185 | 			}
L249186 | 			else if (IsBroken)
L249187 | 			{
L249188 | 				IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
L249189 | 			}
L249190 | 			else if (NetworkSingleton<MoneyManager>.Instance.cashBalance >= 2f)
L249191 | 			{
L249192 | 				IntObj.SetMessage("Purchase Cuke");
L249193 | 				IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
L249194 | 			}
L249195 | 			else
L249196 | 			{
L249197 | 				IntObj.SetMessage("Not enough cash");
L249198 | 				IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
L249199 | 			}
L249200 | 		}
L249201 | 
L249202 | 		public void Interacted()
L249203 | 		{
L249204 | 			if (!purchaseInProgress && !IsBroken && NetworkSingleton<MoneyManager>.Instance.cashBalance >= 2f)
L249205 | 			{
L249206 | 				LocalPurchase();
L249207 | 			}
L249208 | 		}
L249209 | 
L249210 | 		private void LocalPurchase()
L249211 | 		{
L249212 | 			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-2f);
L249213 | 			SendPurchase();
L249214 | 		}
L249215 | 
L249216 | 		[ServerRpc(RequireOwnership = false, RunLocally = true)]
L249217 | 		public void SendPurchase()
L249218 | 		{
L249219 | 			RpcWriter___Server_SendPurchase_2166136261();
L249220 | 			RpcLogic___SendPurchase_2166136261();
L249221 | 		}
L249222 | 
L249223 | 		[ObserversRpc(RunLocally = true)]
L249224 | 		public void PurchaseRoutine()
L249225 | 		{
L249226 | 			RpcWriter___Observers_PurchaseRoutine_2166136261();
L249227 | 			RpcLogic___PurchaseRoutine_2166136261();
L249228 | 		}
L249229 | 
L249230 | 		[ServerRpc(RequireOwnership = false)]
L249231 | 		public void DropItem()
L249232 | 		{

// <<< END OF BLOCK: LINES 249170 TO 249232





// ################################################################################





// >>> START OF BLOCK: LINES 251423 TO 251463
// ----------------------------------------
L251423 | 			reason = string.Empty;
L251424 | 			if (obj is BedItem)
L251425 | 			{
L251426 | 				BedItem bedItem = obj as BedItem;
L251427 | 				if (bedItem.Bed.AssignedEmployee != null)
L251428 | 				{
L251429 | 					reason = "Already assigned to " + bedItem.Bed.AssignedEmployee.fullName;
L251430 | 					return false;
L251431 | 				}
L251432 | 				return true;
L251433 | 			}
L251434 | 			return false;
L251435 | 		}
L251436 | 
L251437 | 		private void UpdateBriefcase()
L251438 | 		{
L251439 | 			Briefcase.gameObject.SetActive(Bed.AssignedEmployee != null || Storage.ItemCount > 0);
L251440 | 			if (Bed.AssignedEmployee != null)
L251441 | 			{
L251442 | 				Storage.StorageEntityName = Bed.AssignedEmployee.FirstName + "'s Briefcase";
L251443 | 				string text = "<color=#54E717>" + MoneyManager.FormatAmount(Bed.AssignedEmployee.DailyWage) + "</color>";
L251444 | 				Storage.StorageEntitySubtitle = Bed.AssignedEmployee.fullName + " will draw " + (Bed.AssignedEmployee.IsMale ? "his" : "her") + " daily wage of " + text + " from this briefcase.";
L251445 | 			}
L251446 | 			else
L251447 | 			{
L251448 | 				Storage.StorageEntityName = "Briefcase";
L251449 | 				Storage.StorageEntitySubtitle = string.Empty;
L251450 | 			}
L251451 | 		}
L251452 | 
L251453 | 		public float GetCashSum()
L251454 | 		{
L251455 | 			float num = 0f;
L251456 | 			foreach (ItemSlot itemSlot in Storage.ItemSlots)
L251457 | 			{
L251458 | 				if (itemSlot.ItemInstance != null && itemSlot.ItemInstance is CashInstance)
L251459 | 				{
L251460 | 					num += (itemSlot.ItemInstance as CashInstance).Balance;
L251461 | 				}
L251462 | 			}
L251463 | 			return num;

// <<< END OF BLOCK: LINES 251423 TO 251463





// ################################################################################





