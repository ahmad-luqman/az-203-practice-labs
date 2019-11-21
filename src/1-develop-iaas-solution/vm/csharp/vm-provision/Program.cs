using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace az203labs.vm_provision
{
    class Program
    {
        static void Main(string[] args)
        {
            //First of all, we need to create a resource group where we will add all the resources
            //needed for the virtual machine.
            var groupName = "az203-ResoureGroup";
            var vmName = "az203VMTesting";
            var location = Region.USWest2;
            var vNetName = "az203VNET";
            var vNetAddress = "172.16.0.0/16";
            var subnetName = "az203Subnet";
            var subnetAddress = "172.16.0.0/24";
            var nicName = "az203NIC";
            var adminUser = "azureadminuser";
            var adminPassword = "Pa$$w0rd!2019";

            //Create the management client. This will be used for all the operations that we will perform in Azure.
            var credentials = SdkContext.AzureCredentialsFactory.FromFile("./azureauth.properties");
            var azure = Azure.Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();

            //We need to create the resource group where we will add the virtual machine.
            var resourceGroup = azure.ResourceGroups.Define(groupName)
                .WithRegion(location)
                .Create();

            //Every virtual machine needs to be connected to a virtual network.
            var network = azure.Networks.Define(vNetName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithAddressSpace(vNetAddress)
                .WithSubnet(subnetName, subnetAddress)
                .Create();

            //Any virtual machine needs a network interface for connecting to the virtual network.
            var nic = azure.NetworkInterfaces.Define(nicName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet(subnetName)
                .WithPrimaryPrivateIPAddressDynamic()
                .Create();
            
            //Create the virtual machine.
            azure.VirtualMachines.Define(vmName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetworkInterface(nic)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                .WithAdminUsername(adminUser)
                .WithAdminPassword(adminPassword)
                .WithComputerName(vmName)
                .WithSize(VirtualMachineSizeTypes.StandardDS2V2)
                .Create();


        }
    }
}
