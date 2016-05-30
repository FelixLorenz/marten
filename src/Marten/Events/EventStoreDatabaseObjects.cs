using System;
using System.IO;
using Marten.Generation;
using Marten.Schema;
using Marten.Services;

namespace Marten.Events
{
    public class EventStoreDatabaseObjects : IDocumentSchemaObjects
    {
        private readonly EventGraph _parent;
        private bool _checkedSchema;
        private readonly object _locker = new object();

        public EventStoreDatabaseObjects(EventGraph parent)
        {
            _parent = parent;
        }

        public void GenerateSchemaObjectsIfNecessary(AutoCreate autoCreateSchemaObjectsMode, IDocumentSchema schema, IDDLRunner runner)
        {
            if (_checkedSchema) return;

            _checkedSchema = true;

            var schemaExists = schema.DbObjects.TableExists(_parent.Table);

            if (schemaExists) return;

            if (autoCreateSchemaObjectsMode == AutoCreate.None)
            {
                throw new InvalidOperationException(
                    "The EventStore schema objects do not exist and the AutoCreateSchemaObjects is configured as " +
                    autoCreateSchemaObjectsMode);
            }

            lock (_locker)
            {
                if (!schema.DbObjects.TableExists(_parent.Table))
                {
                    var writer = new StringWriter();

                    writeBasicTables(schema, writer);

                    runner.Apply(this, writer.ToString());
                }
            }
        }

        public override string ToString()
        {
            return "Event Store";
        }

        private void writeBasicTables(IDocumentSchema schema, StringWriter writer)
        {
            var schemaName = _parent.DatabaseSchemaName;

            writer.WriteSql(schemaName, "mt_stream");
        }

        public void WriteSchemaObjects(IDocumentSchema schema, StringWriter writer)
        {
            writeBasicTables(schema, writer);
        }

        public void RemoveSchemaObjects(IManagedConnection connection)
        {
            throw new NotImplementedException();
        }

        public void ResetSchemaExistenceChecks()
        {
            _checkedSchema = false;
        }

        public TableDefinition StorageTable()
        {
            throw new NotSupportedException();
        }

        public void WritePatch(IDocumentSchema schema, IDDLRunner runner)
        {
            // Nothing yet.
        }
    }
}