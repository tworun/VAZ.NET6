﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAZ.Shared.Utilities.Results
{
	public interface IDataResult<out T> : IResult
	{
		T Data { get; }
	}
}
