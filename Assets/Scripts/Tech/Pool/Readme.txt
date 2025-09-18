Pooling system work with Compoment and game need inject with VContainer
Use prefab or prefab Compoment for Pool
Example: 
	GameObject: PoolManager.Instance.SpawnGameObject(Prefab, position, rotation);
	Compoment: PoolManager.Instnace.SpawnObject<YourCompomentType>(Compoment, position, rotaion);
GenericPool<T> Pool for Class
Structure:
	Pool, ReturnPool and PoolManger class
	ReturnPool: refer t?i Pool và RootComponent
				OnDisalbe() g?i hàm Pool.AddToPool() tr? ??i t??ng v? pool c?a nó say khi nó ?k inactive.
	Pool:	Stack<Object> _inActiveObjs l?u object c?a pool
			Object _baseObj ??i t??ng ???c kh?i t?o ?? ??a vào pool
			GetPool():	check pool còn ??i t??ng nào k n?u có thì _inactiveObjct.pop() ra s? d?ng
						n?u k có kh?i t?o objet m?i thêm cho nó Conponent ReturnPool
						tr??ng h?p 3, ??i t??ng là m?t Componemt
	PoolManager : Singleton:	Dictionary<Object,Pool> _objectPools; qu?n lý các pool theo ??i t??ng kh?i t?o pool
								Dictionary<PoolType, Transform> _poolHolder; qu?n lý các pool trên inspector
								[Inject] IObjectResolver _objectResolver; ??i t??ng ???c Spawn
	1. Prewarm (kh?i t?o s?n m?t s? l??ng object ngay khi b?t ??u)
	2. Lazy initialization (ch? kh?i t?o khi c?n, pool r?ng ban ??u)