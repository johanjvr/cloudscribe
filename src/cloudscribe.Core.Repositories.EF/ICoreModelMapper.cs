﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2015-11-17
// Last Modified:			2015-11-17
// 

using Microsoft.Data.Entity;

namespace cloudscribe.Core.Repositories.EF
{
    public interface ICoreModelMapper
    {
        void DoMapping(ModelBuilder modelBuilder);

    }
}
