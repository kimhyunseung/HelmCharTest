﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConcurrencyTEST
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="WZMobileUser")]
	public partial class WZMobileUserDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region 확장성 메서드 정의
    partial void OnCreated();
    #endregion
		
		public WZMobileUserDataContext() : 
				base(global::ConcurrencyTEST.Properties.Settings.Default.WZMobileUserConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public WZMobileUserDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public WZMobileUserDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public WZMobileUserDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public WZMobileUserDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}

		[global::System.Data.Linq.Mapping.FunctionAttribute(Name = "dbo.USP_MAPI_RegisterGameAccountByPINID")]
		public ISingleResult<USP_MAPI_RegisterGameAccountByPINIDResult> USP_MAPI_RegisterGameAccountByPINID([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ServiceCode", DbType = "Char(6)")] string serviceCode, [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PINID", DbType = "VarChar(20)")] string pINID, [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IP", DbType = "VarChar(39)")] string iP)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), serviceCode, pINID, iP);
			return ((ISingleResult<USP_MAPI_RegisterGameAccountByPINIDResult>)(result.ReturnValue));
		}
	}

	public partial class USP_MAPI_RegisterGameAccountByPINIDResult
	{

		private int _gameAccountNo;

		public USP_MAPI_RegisterGameAccountByPINIDResult()
		{
		}

		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage = "_gameAccountNo", DbType = "Int NOT NULL")]
		public int gameAccountNo
		{
			get
			{
				return this._gameAccountNo;
			}
			set
			{
				if ((this._gameAccountNo != value))
				{
					this._gameAccountNo = value;
				}
			}
		}
	}
}
#pragma warning restore 1591
