﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turan.Shared.Utilities.Results;
using VAZ.Application.Models;

namespace VAZ.Application.Catalog.Bans.Commands.Models
{
	public class UpdateBanCommand: IRequest<IDataResult<BanDto>>
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}