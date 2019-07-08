using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spring.Context;
using Spring.Context.Support;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Testing.Microsoft;
using Spring.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestClass]
    public abstract class SpringLoadTestBase
    {

        private static readonly Object ContextLock = new object();
        private bool populateProtectedVariables = false;
        private AutoWiringMode autowireMode = AutoWiringMode.ByType;
        private bool dependencyCheck = true;
        /// <summary>
        /// Map of context keys returned by subclasses of this class, to
        /// Spring contexts.
        /// </summary>
	    private static readonly IDictionary contextKeyToContextMap;

        /// <summary>
        /// Static ctor to avoid "beforeFieldInit" problem.
        /// </summary>
        static SpringLoadTestBase()
        {
            contextKeyToContextMap = new Hashtable();
        }

        /// <summary>
        /// Disposes any cached context instance and removes it from cache.
        /// </summary>
        public static void ClearContextCache()
        {
            foreach (IApplicationContext ctx in contextKeyToContextMap.Values)
            {
                ctx.Dispose();
            }
            contextKeyToContextMap.Clear();
        }

        /// <summary>
        /// Indicates, whether context instances should be automatically registered with the global <see cref="ContextRegistry"/>.
        /// </summary>
        private bool registerContextWithContextRegistry = true;

        /// <summary>
        /// Logger available to subclasses.
        /// </summary>
        protected readonly ILog logger;

        /// <summary>
        /// Default constructor for AbstractSpringContextTests.
        /// </summary>
        protected SpringLoadTestBase()
        {
            logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Controls, whether application context instances will
        /// be registered/unregistered with the global <see cref="ContextRegistry"/>.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool RegisterContextWithContextRegistry
        {
            get { return registerContextWithContextRegistry; }
            set { registerContextWithContextRegistry = value; }
        }

        /// <summary>
        /// Set custom locations dirty. This will cause them to be reloaded
        /// from the cache before the next test case is executed.
        /// </summary>
        /// <remarks>
        /// Call this method only if you change the state of a singleton
        /// object, potentially affecting future tests.
        /// </remarks>
        /// <param name="locations">Locations </param>
	    protected void SetDirty(string[] locations)
        {
            String keyString = ContextKeyString(locations);
            IConfigurableApplicationContext ctx =
                    (IConfigurableApplicationContext)contextKeyToContextMap[keyString];
            contextKeyToContextMap.Remove(keyString);

            if (ctx != null)
            {
                ctx.Dispose();
            }
        }

        /// <summary>
        /// Returns <c>true</c> if context for the specified 
        /// <paramref name="contextKey"/> is cached.
        /// </summary>
        /// <param name="contextKey">Context key to check.</param>
        /// <returns>
        /// <c>true</c> if context for the specified 
        /// <paramref name="contextKey"/> is cached, 
        /// <c>false</c> otherwise.
        /// </returns>
	    protected bool HasCachedContext(object contextKey)
        {
            string keyString = ContextKeyString(contextKey);
            return contextKeyToContextMap.Contains(keyString);
        }

        /// <summary>
        /// Converts context key to string.
        /// </summary>
        /// <remarks>
        /// Subclasses can override this to return a string representation of
        /// their contextKey for use in logging.
        /// </remarks>
        /// <param name="contextKey">Context key to convert.</param>
        /// <returns>
        /// String representation of the specified <paramref name="contextKey"/>.  Null if 
        /// contextKey is null.
        /// </returns>
	    protected virtual string ContextKeyString(object contextKey)
        {
            if (contextKey == null)
            {
                return null;
            }
            if (contextKey is string[])
            {
                return StringUtils.CollectionToCommaDelimitedString((string[])contextKey);
            }
            else
            {
                return contextKey.ToString();
            }
        }

        /// <summary>
        /// Caches application context.
        /// </summary>
        /// <param name="key">Key to use.</param>
        /// <param name="context">Context to cache.</param>
	    public void AddContext(object key, IConfigurableApplicationContext context)
        {
            AssertUtils.ArgumentNotNull(context, "context", "ApplicationContext must not be null");
            string keyString = ContextKeyString(key);
            if (contextKeyToContextMap.Contains(keyString))
                return;
            lock (ContextLock)
            {
                contextKeyToContextMap.Add(keyString, context);

                if (RegisterContextWithContextRegistry
                    && !ContextRegistry.IsContextRegistered(context.Name))
                {
                    ContextRegistry.RegisterContext(context);
                }
            }

        }

        /// <summary>
        /// Returns cached context if present, or loads it if not.
        /// </summary>
        /// <param name="key">Context key.</param>
        /// <returns>Spring application context associated with the specified key.</returns>
	    protected IConfigurableApplicationContext GetContext(object key)
        {
            string keyString = ContextKeyString(key);
            IConfigurableApplicationContext ctx = (IConfigurableApplicationContext)contextKeyToContextMap[keyString];
            if (ctx == null)
            {
                if (key is string[])
                {
                    ctx = LoadContextLocations((string[])key);
                }
                else
                {
                    ctx = LoadContext(key);
                }
                AddContext(key, ctx);
            }
            return ctx;
        }


        /// <summary>
        /// Loads application context based on user-defined key.
        /// </summary>
        /// <remarks>
        /// Unless overriden by the user, this method will alway throw 
        /// a <see cref="NotSupportedException"/>. 
        /// </remarks>
        /// <param name="key">User-defined key.</param>
        protected virtual IConfigurableApplicationContext LoadContext(object key)
        {
            throw new NotSupportedException("Subclasses may override this");
        }

        /// <summary>
        /// Application context this test will run against.
        /// </summary>
        protected IConfigurableApplicationContext applicationContext;

        /// <summary>
        /// Holds names of the fields that should be used for field injection.
        /// </summary>
        protected string[] managedVariableNames;
        private int loadCount = 0;

        /// <summary>
        /// Gets or sets a flag specifying whether to populate protected 
        /// variables of this test case.
        /// </summary>
        /// <value>
        /// A flag specifying whether to populate protected variables of this test case. 
        /// Default is <b>false</b>.
        /// </value>
        public bool PopulateProtectedVariables
        {
            get { return populateProtectedVariables; }
            set { populateProtectedVariables = value; }
        }

        /// <summary>
        /// Gets or sets the autowire mode for test properties set by Dependency Injection.
        /// </summary>
        /// <value>
        /// The autowire mode for test properties set by Dependency Injection.
        /// The default is <see cref="AutoWiringMode.ByType"/>.
        /// </value>
        public AutoWiringMode AutowireMode
        {
            get { return autowireMode; }
            set { autowireMode = value; }
        }

        /// <summary>
        /// Gets or sets a flag specifying whether or not dependency checking 
        /// should be performed for test properties set by Dependency Injection.
        /// </summary>
        /// <value>
        /// <p>A flag specifying whether or not dependency checking 
        /// should be performed for test properties set by Dependency Injection.</p>
        /// <p>The default is <b>true</b>, meaning that tests cannot be run
        /// unless all properties are populated.</p>
        /// </value>
        public bool DependencyCheck
        {
            get { return dependencyCheck; }
            set { dependencyCheck = value; }
        }

        /// <summary>
        /// Gets the current number of context load attempts.
        /// </summary>
        public int LoadCount
        {
            get { return loadCount; }
        }

        /// <summary>
        /// Called to say that the "applicationContext" instance variable is dirty and
        /// should be reloaded. We need to do this if a test has modified the context
        /// (for example, by replacing an object definition).
        /// </summary>
        public void SetDirty()
        {
            SetDirty(ConfigLocations);
        }

        /// <summary>
        /// Test setup method.
        /// </summary>
        [TestInitialize]
        public virtual void TestInitialize()
        {
            this.applicationContext = GetContext(ContextKey);
            InjectDependencies();
            try
            {
                OnTestInitialize();
            }
            catch (Exception ex)
            {
                logger.Error("Setup error", ex);
                throw;
            }
        }

        /// <summary>
        /// Inject dependencies into 'this' instance (that is, this test instance).
        /// </summary>
        /// <remarks>
        /// <p>The default implementation populates protected variables if the
        /// <see cref="PopulateProtectedVariables"/> property is set, else
        /// uses autowiring if autowiring is switched on (which it is by default).</p>
        /// <p>You can certainly override this method if you want to totally control
        /// how dependencies are injected into 'this' instance.</p>
        /// </remarks>
        protected virtual void InjectDependencies()
        {
            if (PopulateProtectedVariables)
            {
                if (this.managedVariableNames == null)
                {
                    InitManagedVariableNames();
                }
                InjectProtectedVariables();
            }
            else if (AutowireMode != AutoWiringMode.No)
            {
                IConfigurableListableObjectFactory factory = this.applicationContext.ObjectFactory;
                ((AbstractObjectFactory)factory).IgnoreDependencyType(typeof(AutoWiringMode));
                factory.AutowireObjectProperties(this, AutowireMode, DependencyCheck);
            }
        }

        /// <summary>
        /// Gets a key for this context. Usually based on config locations, but
        /// a subclass overriding buildContext() might want to return its class.
        /// </summary>
        protected virtual object ContextKey
        {
            get { return ConfigLocations; }
        }

        /// <summary>
        /// Loads application context from the specified resource locations.
        /// </summary>
        /// <param name="locations">Resources to load object definitions from.</param>
        protected IConfigurableApplicationContext LoadContextLocations(string[] locations)
        {
            ++this.loadCount;

            if (logger.IsInfoEnabled)
            {
                logger.Info("Loading config for: " + StringUtils.CollectionToCommaDelimitedString(locations));
            }
            return new XmlApplicationContext(locations);
        }

        /// <summary>
        /// Retrieves the names of the fields that should be used for field injection.
        /// </summary>
        protected virtual void InitManagedVariableNames()
        {
            List<string> managedVarNames = new List<string>();
            Type type = GetType();

            do
            {
                FieldInfo[] fields =
                    type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance);
                if (logger.IsDebugEnabled)
                {
                    logger.Debug("Found " + fields.Length + " fields on " + type);
                }

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (logger.IsDebugEnabled)
                    {
                        logger.Debug("Candidate field: " + field);
                    }
                    if (IsProtectedInstanceField(field))
                    {
                        object oldValue = field.GetValue(this);
                        if (oldValue == null)
                        {
                            managedVarNames.Add(field.Name);
                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug("Added managed variable '" + field.Name + "'");
                            }
                        }
                        else
                        {
                            if (logger.IsDebugEnabled)
                            {
                                logger.Debug("Rejected managed variable '" + field.Name + "'");
                            }
                        }
                    }
                }
                type = type.BaseType;
            } while (type != typeof(AbstractDependencyInjectionSpringContextTests));

            this.managedVariableNames = managedVarNames.ToArray();
        }

        private static bool IsProtectedInstanceField(FieldInfo field)
        {
            return field.IsFamily;
        }

        /// <summary>
        /// Injects protected fields using Field Injection.
        /// </summary>
        protected virtual void InjectProtectedVariables()
        {
            for (int i = 0; i < this.managedVariableNames.Length; i++)
            {
                string fieldName = this.managedVariableNames[i];
                Object obj = null;
                try
                {
                    FieldInfo field = GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        BeforeProtectedVariableInjection(field);
                        obj = this.applicationContext.GetObject(fieldName, field.FieldType);
                        field.SetValue(this, obj);
                        if (logger.IsDebugEnabled)
                        {
                            logger.Debug("Populated field: " + field);
                        }
                    }
                    else
                    {
                        if (logger.IsWarnEnabled)
                        {
                            logger.Warn("No field with name '" + fieldName + "'");
                        }
                    }
                }
                catch (NoSuchObjectDefinitionException)
                {
                    if (logger.IsWarnEnabled)
                    {
                        logger.Warn("No object definition with name '" + fieldName + "'");
                    }
                }
            }
        }

        /// <summary>
        /// Called right before a field is being injected
        /// </summary>
        protected virtual void BeforeProtectedVariableInjection(FieldInfo fieldInfo)
        {

        }

        /// <summary>
        /// Test teardown method.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                //OnTestCleanup();
            }
            catch (Exception ex)
            {
                logger.Error("OnTearDown error", ex);
            }
        }


        /// <summary>
        /// Subclasses must implement this property to return the locations of their
        /// config files. A plain path will be treated as a file system location.
        /// </summary>
        /// <value>An array of config locations</value>
        protected abstract string[] ConfigLocations { get; }



        /// <summary>
        /// Creates a transaction
        /// </summary>
        protected void OnTestInitialize()
        {
            /*
            this.complete = !this.defaultRollback;

            if (this.transactionManager == null)
            {
                logger.Info("No transaction manager set: test will NOT run within a transaction");
            }
            else if (this.transactionDefinition == null)
            {
                logger.Info("No transaction definition set: test will NOT run within a transaction");
            }
            else
            {
                //OnSetUpBeforeTransaction();
                //StartNewTransaction();
                try
                {
                    OnSetUpInTransaction();
                }
                catch (Exception)
                {
                    EndTransaction();
                    throw;
                }
            }
            */
        }


    }
}
