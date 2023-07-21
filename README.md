# Migratonator - Database Migration Tool

[![.NET](https://github.com/cstefano/mgatr/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cstefano/mgatr/actions/workflows/dotnet.yml)

_`Migrate - toe - nater`_  (_`EM gatt err`_ for short)

> CLI Tool for managing MSSQL Database schemas, objects and data.

Built to leverage the excellent [DbUp](https://dbup.github.io/) library, `Migratonator` adds
support for managing variables, migration files, running scripts for views and stored procedures,
adds support for SQL CLR Assemblies and the use of macros in scripts.

## Concepts

The tool is opinionated such that:

* a specific directory structure is assumed,
* the order in which changes are applied is deterministic,
* changes which affect the data, such as to types, tables and indexes, must be made using migrations,
* changes to all other objects, such as views, functions and procedures must be scripted in an idempotent way,
* data seeds must be scripted in an idempotent way,
* a `Schema.sql` file is generated to represent the most current version of the database schema.

### Workflow

The are three common use-cases for the tool, each within a different environment, and where each
process is subtly different.

#### Development

The development workflow commences by creating a local isolated database. Next, migrations are
written to define types, tables, constraints, indexes, and other necessary objects, which are
then applied to the database. Subsequently, additional scripts are developed to create functions,
stored procedures and views, etc, and these changes are also applied to the database.

The application code is then tested against the modified database, and an iterative process begins.
During this stage, the developer works to enhance or fix the migrations and scripts, making use of
the capability to undo and redo changes as required, until the application functions as desired.

Once satisfied with the changes, the developer commits them to the source code control system and
proceeds to create a pull request. The purpose of the pull request is to have the changes reviewed
and tested by Continuous Integration process before it is merged into the main codebase.

The tool utilised in this workflow enables the developer to easily tear down the local environment,
facilitating the creation of a fresh environment for working from a known state, or for working on
a different branch of the code to allow for further testing and development on that branch.

Having the ability to tear down and recreate the local environment, developers can adhere to a
more structured and deterministic approach to developing applications. It ensures that the
development process is better organized, follows a repeatable process, which leads to more reliable
and predictable outcomes.

The tool provides the following commands for this workflow.

* `mgatr init` - Initialise the project directory structure and configuration for the tool to work.
* `mgatr db create` - Create a new empty database.
* `mgatr schema load` - Load the base version of schema into the database.
* `mgatr migrate create <Name>` - Create a new migration, with a name which describes the purpose
  of the migration, such as "Add X Column to Table Y", or "Add Unique Index to Table X", etc.
* `mgatr migrate up` - Apply the migration to the database[^2].
* `mgatr migrate scripts` - Apply script changes to the database[^2].
* `mgatr schema dump` - Generates the `Schema.sql` file.
* `mgatr db drop` - Deletes the database, to enable the developer to start again if needed.

Optionally, if the database needs to contain a working set of the data, for developers to work on
the application code and to run integration tests, the scripts in the `Seeds` directory can be
applied to the database using the `mgatr seeds` command.

#### Continuous Integration

Once the pull request has been submitted, the Continuous Integration process automatically picks
up the changes and runs a suite of automated tests to validate the integrity and compatibility
of the change with the existing codebase. This helps ensure seamless integration of changes into the
project while maintaining overall stability and functionality.

This process consists of two checks. The first step involves verifying that the migrations and
scripts work against the existing database version (as in production), since this step will be
performed during deployment, and secondly to check that the application works as intended using
this new version of the database.

Verifing that the migrations and scripts work involes the following steps.

* Check out the version of the `Schema.sql` file as at the start[^1] of the pull request.
* `mgatr db create` - Create a new empty database.
* `mgatr schema load` - Load the base version of schema into the database.
* `mgatr migrate up` - Apply the migrations.
* `mgatr migrate scripts` - Apply the script changes.
* Assert that the generated `Schema.sql` file is the same as that of the pull request[^3].
* Optionally, run `mgatr seeds` to seed the database with data required for the execution
  the applications integration tests.
* `mgatr db drop` - Deletes the database, to free up resources in the CI environment.

#### Deployment

After pull requests have been merged, and a release has been prepared, the deployment process
which runs in the respective deployed environments, such as developer, staging and production,
uses the tool to apply the changes to the running databases.

This process involves the following steps.

* `mgatr migrate up` - Apply the migrations.
* `mgatr migrate scripts` - Apply the script changes.

### Directory Structure

The directory structure and base files are as follows.

```
Project Root Directory
  ∟ Assemblies
    - Assembly.sql
    - ...
  ∟ Functions
    - Function.sql
    - ...
  ∟ Jobs
    - Job.sql
    - ...
  ∟ Migrations
    - 0001-Migration1.sql
    - 0002-Migration2.sql
    - ...
    - 9999-Migration999.sql
  ∟ Procedures
    - Procedure.sql
    - ...
  ∟ Seeds
    - Seed.sql
    - ...
  ∟ Views
    - View.sql
    - ...
  - Schema.sql
  - Variables.json
  - Macros.json
```

### Execution

The order in which schema, object and data changes are be made in the following order.

1. Migrations
1. Assemblies
1. Functions
1. Procedures
1. Views
1. Jobs
1. Seeds

### Migrations

Scripts within the `Migrations` directory will only be executed once, whereas _all_ other
scripts will be executed each time and therefore must be implemented to be _idempotent_.

Furthermore, scripts within the `Migrations` directory _must only_ be used to manage objects
which hold or where there are hard dependencies.

Examples include:

* creating types,
* creating tables,
* modifying columns,
* modifying constraints, indexes,
* and dropping existing objects.

By following this convention, dependencies between scripts can be managed deterministically.

### Scripts

### `Schema.sql`

The `Schema.sql` file is generated by the tool each time a command which effects a change is run.

It is used to recreate the database for the current version, and should be checked into source code
control after migrations and scripts have been applied in the development environment.

The updated `Schema.sql` file should be included together with the corresponding migrations as
part of a pull request, so that the full extent of the change can understood by the reviewer.

### Variables

Scripts can contain variables which get replaced when the migrations are prepared.

Use the format `$VariableName$` to denote a variable in the scripts and provide the
corresponding named entries in the `Variables.json` file.

E.g. Given a script which creates a user role:

```sql
CREATE LOGIN [$DomainName$\$UserName$]
FROM WINDOWS
;

GO
```

And the variables JSON file contents:

```json
[
  {
    "Name": "DomainName",
    "Value": "SOMEDOMAIN"
  },
  {
    "Name": "UserName",
    "Value": "Bob"
  }
]
```

The rendered script would be as follows:

```sql
CREATE LOGIN [SOMEDOMAIN\Bob]
FROM WINDOWS
;

GO
```

### SQL CLR Assemblies

_To be completed_

### Macros

Macros are provided via the `Macros.json` file. Macros are inserted into scripts using
the `/* %Macro:Name[Parameter1|Parameter2|...|ParameterN%] */` syntax.

E.g. Psuedo T-SQL code for a hypothetical `OBJECT` type.

```sql
-- %Macro:DropObjectIfExist[ObjectName]%

CREATE OBJECT ObjectName AS
...
```

When migrations or scripts are applied, the macros are replaced with the respective
templated content before execution.

## Pre-requisites

* [DotNet Core][dotnet]
* [`DbUp` Library][dbup]
* [`System.CommandLine` Library][cmdline]
* [`Microsoft.SqlServer.SqlManagementObjects` Library][smo]

## Usage

See [commands](COMMANDS.md) for the commandline reference.

## Hacking

### Building

Build the tool using the `dotnet` command line tool.

```bash
dotnet build
```

### Testing

Test the tool using the `dotnet` command line tool.

```bash
dotnet test
```

### Running

Run the tool with the `--help` flag to see the available options.

```bash
mgatr --help
```

## License

[MIT License](LICENSE) Copyright (c) Chris Stefano

<!-- footnotes -->

[^1]: The [`git merge-base`][mergebase] command can be used to determine the branch point commit.
[^2]: This command automatically generates a revised `Scheme.sql` script.
[^3]: Helps to enforce that the developer followed the correct flow.

<!-- links -->

[cmdline]: https://www.nuget.org/packages/System.CommandLine
[dbup]: https://github.com/DbUp/DbUp
[dotnet]: https://dotnet.microsoft.com/download
[mergebase]: https://git-scm.com/docs/git-merge-base/
[smo]: https://www.nuget.org/packages/Microsoft.SqlServer.SqlManagementObjects
