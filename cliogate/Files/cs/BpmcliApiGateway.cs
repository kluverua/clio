﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using ClioGate.Functions.SQL;
using Newtonsoft.Json;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.ConfigurationBuild;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Factories;
using Terrasoft.Core.Packages;
using Terrasoft.Web.Common;
#if NETSTANDARD2_0
using System.Globalization;
using Terrasoft.Web.Http.Abstractions;
#endif

namespace cliogate.Files.cs
{

	#region Class: CompilationResult

	[DataContract]
	public class CompilationResult
	{
		[JsonProperty("compilerErrors")]
		public CompilerErrorCollection CompilerErrors { get; set; }

		[JsonProperty("status")]
		public BuildResultType Status { get; set; }
	}

	#endregion

	#region Class: CreatioApiGateway

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class CreatioApiGateway : BaseService
	{

		#region Methods: Private

		private IEnumerable<Guid> GetEntitySchemasWithNeedUpdateStructure() {
			var result = new List<Guid>();
			var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "VwSysEntitySchemaInWorkspace");
			esq.AddAllSchemaColumns();
			esq.Filters.LogicalOperation = Terrasoft.Common.LogicalOperationStrict.And;
			var needUpdateFilter = esq.CreateFilterWithParameters(FilterComparisonType.Equal, "NeedUpdateStructure", true);
			esq.Filters.Add(needUpdateFilter);
			var packages = esq.GetEntityCollection(UserConnection);
			foreach (var p in packages) {
				var schemaUId = p.GetTypedColumnValue<Guid>("UId");
				result.Add(schemaUId);
			}
			return result;
		}

		private PackageInstallUtilities CreateInstallUtilities() {
			return new PackageInstallUtilities(UserConnection);
		}

		private void AssignFileResponseContent(string contentType, long contentLength,
			string contentDisposition) {
#if !NETSTANDARD2_0
			WebOperationContext.Current.OutgoingResponse.ContentType = contentType;
			WebOperationContext.Current.OutgoingResponse.ContentLength = contentLength;
			WebOperationContext.Current.OutgoingResponse.Headers.Add("Content-Disposition", contentDisposition);
#else
			HttpContext httpContext = HttpContextAccessor.GetInstance();
			HttpResponse response = httpContext.Response;
			response.ContentType = contentType;
			response.Headers["Content-Length"] = contentLength.ToString(CultureInfo.InvariantCulture);
			response.Headers["Content-Disposition"] = contentDisposition;
#endif
		}

		private void AssignFileResponseContent(long contentLength, string fileName) =>
			AssignFileResponseContent( "application/octet-stream", contentLength,
				$"attachment; filename=\"{fileName}\"");

		private Stream GetCompressedFolder(string rootRelativePath, string fileName) {
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string rootDirectoryPath = Path.Combine(baseDirectory, rootRelativePath);
			MemoryStream compressedAutogeneratedFolder = CompressionUtilities.GetCompressedFolder(rootDirectoryPath);
			AssignFileResponseContent(compressedAutogeneratedFolder?.Length ?? 0,fileName);
			return compressedAutogeneratedFolder;
		}

		#endregion

		#region Methods: Public

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "ExecuteSqlScript", BodyStyle = WebMessageBodyStyle.WrappedRequest,
			RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public string ExecuteSqlScript(string script) {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				return SQLFunctions.ExecuteSQL(script, UserConnection);
			} else {
				throw new Exception("You don't have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "GET", UriTemplate = "GetApiVersion", RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
		public string GetApiVersion() {
			var version = typeof(CreatioApiGateway).Assembly.GetName().Version;
			return version.ToString();
		}

		[OperationContract]
		[WebGet(UriTemplate = "GetEntitySchemaModels/{entitySchema}/{fields}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
		public string GetEntitySchemaModels(string entitySchema, string fields) {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				var generator = new EntitySchemaModelClassGenerator(UserConnection.EntitySchemaManager);
				var columns = new List<string>();
				if (!String.IsNullOrEmpty(fields)) {
					columns = new List<string>(fields.Split(','));
				}
				var models = generator.Generate(entitySchema, columns);
				return JsonConvert.SerializeObject(models, Formatting.Indented);
			} else {
				throw new Exception("You don't have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "GetPackages", BodyStyle = WebMessageBodyStyle.WrappedRequest,
		RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public string GetPackages(bool isCustomer = false) {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				var packageList = new List<Dictionary<string, string>>();
				var esq = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "SysPackage");
				esq.AddAllSchemaColumns();
				if (isCustomer) {
					var maintainerName = SysSettings.GetValue(UserConnection, "Maintainer", string.Empty);
					var customPackageUId = SysSettings.GetValue(UserConnection, "CustomPackageUId", Guid.Empty);
					IEntitySchemaQueryFilterItem maintainerNameFilter =
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Maintainer", maintainerName);
					IEntitySchemaQueryFilterItem nameNotEqualCustomFilter =
						esq.CreateFilterWithParameters(FilterComparisonType.NotEqual, "UId", customPackageUId);
					IEntitySchemaQueryFilterItem installTypeFilter =
						esq.CreateFilterWithParameters(FilterComparisonType.Equal, "InstallType", 0);
					esq.Filters.Add(maintainerNameFilter);
					esq.Filters.Add(nameNotEqualCustomFilter);
					esq.Filters.Add(installTypeFilter);
				}
				var packages = esq.GetEntityCollection(UserConnection);
				foreach (var p in packages) {
					var package = new Dictionary<string, string> {
						["Name"] = p.PrimaryDisplayColumnValue,
						["UId"] = p.GetTypedColumnValue<string>("UId"),
						["Maintainer"] = p.GetTypedColumnValue<string>("Maintainer"),
						["Version"] = p.GetTypedColumnValue<string>("Version")
					};
					packageList.Add(package);
				}
				var json = JsonConvert.SerializeObject(packageList);
				return json;
			} else {
				throw new Exception("You don'nt have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "ResetSchemaChangeState",
			BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public bool ResetSchemaChangeState(string packageName) {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				Update update = new Update(UserConnection, "SysSchema")
					.Set("IsChanged", Column.Parameter(false, "Boolean"))
					.Where("SysPackageId").In(new Select(UserConnection).Column("Id").From("SysPackage")
				.Where("SysPackage", "Name").IsEqual(Column.Parameter(packageName))) as Update;
				update.Execute();
				return true;
			}
			else {
				throw new Exception("You don'nt have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "UnlockPackages",
			BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public bool UnlockPackages(string[] unlockPackages = null) {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				var maintainerCode = SysSettings.GetValue<string>(UserConnection, "Maintainer", "NonImplemented");
				Query query = new Update(UserConnection, "SysPackage")
					.Set("InstallType", Column.Parameter(0, "Integer"))
					.Where("Maintainer").IsEqual(Column.Parameter(maintainerCode)) as Update;
				if (unlockPackages != null && unlockPackages.Any()) {
					query = query.And("Name").In(Column.Parameters((IEnumerable<string>)unlockPackages));
				}
				Update update = query as Update;
				update.Execute();
				return true;
			} else {
				throw new Exception("You don'nt have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "LockPackages",
			BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json,
			ResponseFormat = WebMessageFormat.Json)]
		public bool LockPackages(string[] lockPackages = null) {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				var maintainerCode = SysSettings.GetValue<string>(UserConnection, "Maintainer", "NonImplemented");
				Query query = new Update(UserConnection, "SysPackage")
					.Set("InstallType", Column.Parameter(1, "Integer"))
					.Where("Maintainer").IsEqual(Column.Parameter(maintainerCode))
					.And("Name").IsNotEqual(Column.Parameter("Custom")) as Update;
				if (lockPackages != null && lockPackages.Any()) {
					query = query.And("Name").In(Column.Parameters((IEnumerable<string>)lockPackages));
				}
				Update update = query as Update;
				update.Execute();
				return true;
			} else {
				throw new Exception("You don'nt have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "CompileWorkspace", BodyStyle = WebMessageBodyStyle.WrappedRequest,
		RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public CompilationResult CompileWorkspace(bool compileModified = false) {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				WorkspaceBuilder workspaceBuilder = WorkspaceBuilderUtility.CreateWorkspaceBuilder(AppConnection);
				CompilerErrorCollection compilerErrors = workspaceBuilder.Rebuild(AppConnection.Workspace,
					out var buildResultType);
				var configurationBuilder = ClassFactory.Get<IAppConfigurationBuilder>();
				if (compileModified) {
					configurationBuilder.BuildChanged();
				} else {
					configurationBuilder.BuildAll();
				}
				return new CompilationResult {
					Status = buildResultType,
					CompilerErrors = compilerErrors
				};
			} else {
				throw new Exception("You don't have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "UpdateDBStructure", BodyStyle = WebMessageBodyStyle.WrappedRequest,
		RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public bool UpdateDBStructure() {
			if (UserConnection.DBSecurityEngine.GetCanExecuteOperation("CanManageSolution")) {
				var invalidSchemas = GetEntitySchemasWithNeedUpdateStructure();
				return CreateInstallUtilities().SaveSchemaDBStructure(invalidSchemas, true);
			} else {
				throw new Exception("You don't have permission for operation CanManageSolution");
			}
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public Stream GetAutogeneratedFolder(string packageName){
			string rootRelativePath = Path.Combine("Terrasoft.Configuration", "Pkg",
				packageName, "Autogenerated");
			return Directory.Exists(rootRelativePath)
				? GetCompressedFolder(rootRelativePath, $"Autogenerated.{packageName}.gz")
				: null;
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public Stream GetConfigurationBinFolder() {
			string rootRelativePath = Path.Combine("Terrasoft.Configuration", "bin");
			return GetCompressedFolder(rootRelativePath, $"Bin.gz");
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public Stream GetConfigurationLibFolder() {
			string rootRelativePath = Path.Combine("Terrasoft.Configuration", "Lib");
			return GetCompressedFolder(rootRelativePath, $"Lib.gz");
		}

		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		public Stream GetCoreBinFolder(){
			string rootRelativePath = "bin";
			return GetCompressedFolder(rootRelativePath, $"CoreBin.gz");
		}

		#endregion

	}

	#endregion

}