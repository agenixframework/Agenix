namespace Agenix.Core
{
    public abstract class AbstractTestActionBuilder<T, S> : ITestActionBuilder<T>
    where T : ITestAction
    where S : ITestActionBuilder<T>
    {
        protected S self;

        private string name;
        private string description;

        protected AbstractTestActionBuilder()
        {
            self = (S)(object)this;
        }

        /**
         * Sets the test action name.
         * @param name the test action name.
         * @return
         */
        public S Name(string name)
        {
            this.name = name;
            return self;
        }

        /**
         * Sets the description.
         * @param description
         * @return
         */
        public S Description(string description)
        {
            this.description = description;
            return self;
        }

        /**
         * Obtains the name.
         * @return
         */
        public string GetName()
        {
            return name;
        }

        /**
         * Obtains the description.
         * @return
         */
        public string GetDescription()
        {
            return description;
        }

        public abstract T Build();
    }
}