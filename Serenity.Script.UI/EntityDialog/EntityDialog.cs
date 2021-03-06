﻿using System;
using System.Collections;
using jQueryApi.UI.Widgets;
using jQueryApi;
using System.Threading.Tasks;

namespace Serenity
{
    public interface IEditDialog : IDialog
    {
        void Load(object entityOrId, Action done, Action<object> fail);
    }

    public abstract partial class EntityDialog<TEntity, TOptions> : TemplatedDialog<TOptions>, IEditDialog
        where TEntity : class, new()
        where TOptions: class, new()
    {
        private TEntity entity;
        private Int64? entityId;
        
        protected EntityDialog()
            : this(Q.NewBodyDiv(), null)
        {
        }

        protected EntityDialog(TOptions opt)
            : this(Q.NewBodyDiv(), opt)
        {
        }

        protected EntityDialog(jQueryObject div, TOptions opt)
            : base(div, opt)
        {
            if (!IsAsyncWidget())
            {
                #pragma warning disable 618
                InitPropertyGrid();
                InitLocalizationGrid();
                #pragma warning restore 618
            }
        }

        protected override Promise InitializeAsync()
        {
            return base.InitializeAsync()
                .ThenAwait(InitPropertyGridAsync)
                .ThenAwait(InitLocalizationGridAsync);
        }

        public override void Destroy()
        {
            if (propertyGrid != null)
            {
                propertyGrid.Destroy();
                propertyGrid = null;
            }

            if (localizationGrid != null)
            {
                localizationGrid.Destroy();
                localizationGrid = null;
            }

            this.undeleteButton = null;
            this.applyChangesButton = null;
            this.deleteButton = null;
            this.saveAndCloseButton = null;

            base.Destroy();
        }

        protected TEntity Entity
        {
            get { return entity; }
            set { entity = value ?? new TEntity(); }
        }

        protected internal Nullable<Int64> EntityId
        {
            get { return entityId; }
            set { entityId = value; }
        }

        protected virtual string GetEntityNameFieldValue()
        {
            return (Entity.As<JsDictionary>()[GetEntityNameField()] ?? "").ToString();
        }

        protected virtual string GetEntityTitle()
        {
            if (!(this.IsEditMode))
                return String.Format(Texts.Controls.EntityDialog.NewRecordTitle, GetEntitySingular());
            else
            {
                string title = (GetEntityNameFieldValue() ?? "");
                return String.Format(Texts.Controls.EntityDialog.EditRecordTitle, GetEntitySingular(),
                    (title.IsEmptyOrNull() ? "" : " (" + title + ")"));
            }
        }

        protected virtual void UpdateTitle()
        {
            if (!isPanel)
                element.Dialog().Title = GetEntityTitle();
        }

        protected override string GetTemplateName()
        {
            var templateName = base.GetTemplateName();
            
            if (!Q.CanLoadScriptData("Template." + templateName) &&
                Q.CanLoadScriptData("Template.EntityDialog"))
                return "EntityDialog";

            return templateName;
        }

        protected bool IsCloneMode
        {
            get { return EntityId != null && IdExtensions.IsNegativeId(EntityId.Value); }
        }

        protected bool IsEditMode
        {
            get { return EntityId != null && IdExtensions.IsPositiveId(EntityId.Value); }
        }

        protected bool IsDeleted
        {
            get 
            { 
                if (EntityId == null)
                    return false;

                var value = Entity.As<JsDictionary>()[GetEntityIsActiveField()].As<Int32?>();
                if (value == null)
                    return false;

                return IdExtensions.IsNegativeId(value.Value);
            }
        }

        protected bool IsNew
        {
            get { return EntityId == null; }
        }

        protected bool IsNewOrDeleted
        {
            get { return IsNew || this.IsDeleted; }
        }
    }

    public abstract class EntityDialog<TEntity> : EntityDialog<TEntity, object>
        where TEntity : class, new()
    {
        protected EntityDialog()
            : base(null)
        {
        }
    }
}