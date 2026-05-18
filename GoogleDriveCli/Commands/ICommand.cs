using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleDriveCli.Commands;
public interface ICommand
{
    Task Execute(string[] args);
}
