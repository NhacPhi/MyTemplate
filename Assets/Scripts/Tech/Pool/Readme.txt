Pooling system work with Compoment and game need inject with VContainer
Use prefab or prefab Compoment for Pool
Example: 
	GameObject: PoolManager.Instance.SpawnGameObject(Prefab, position, rotation);
	Compoment: PoolManager.Instnace.SpawnObject<YourCompomentType>(Compoment, position, rotaion);
GenericPool<T> Pool for Class
Structure:
	Pool, ReturnPool and PoolManger class
	ReturnPool: refer t?i Pool v� RootComponent
				OnDisalbe() g?i h�m Pool.AddToPool() tr? ??i t??ng v? pool c?a n� say khi n� ?k inactive.
	Pool:	Stack<Object> _inActiveObjs l?u object c?a pool
			Object _baseObj ??i t??ng ???c kh?i t?o ?? ??a v�o pool
			GetPool():	check pool c�n ??i t??ng n�o k n?u c� th� _inactiveObjct.pop() ra s? d?ng
						n?u k c� kh?i t?o objet m?i th�m cho n� Conponent ReturnPool
						tr??ng h?p 3, ??i t??ng l� m?t Componemt
	PoolManager : Singleton:	Dictionary<Object,Pool> _objectPools; qu?n l� c�c pool theo ??i t??ng kh?i t?o pool
								Dictionary<PoolType, Transform> _poolHolder; qu?n l� c�c pool tr�n inspector
								[Inject] IObjectResolver _objectResolver; ??i t??ng ???c Spawn
	1. Prewarm (kh?i t?o s?n m?t s? l??ng object ngay khi b?t ??u)
	2. Lazy initialization (ch? kh?i t?o khi c?n, pool r?ng ban ??u)