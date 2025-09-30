// MIT License
// Copyright $ext_companyname$

using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using System;
using System.Collections.Generic;

namespace $ext_rootnamespace$.MenuControllers
{
    /// <summary>
    /// Menu controller for the $ext_safeprojectname$ module.
    /// </summary>
    /// <seealso cref="Dnn.PersonaBar.Library.Controllers.IMenuItemController" />
    public class $ext_safeprojectname$MenuController : IMenuItemController
    {
        /// <inheritdoc/>
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            var settings = new Dictionary<string, object>();
            settings.Add("SampleSetting", "SampleValue");
            return settings;
        }

        /// <inheritdoc/>
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        /// <inheritdoc/>
        public bool Visible(MenuItem menuItem)
        {
            return true;
        }
    }
}
