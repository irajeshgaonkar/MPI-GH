# Bruno MPI API Tests

## Overview
This folder contains the Bruno MPI API tests for both **DEV** and **INT** environments.  
It is designed to allow developers and testers to quickly run Automation tests in Bruno.


### Explanation of Structure
- **collections/P0_TestsCases/**: Contains the main P0 test collection.
  - Each subfolder represents a specific API and contains multiple test cases.
  -**collections/P1_TestsCases/**: Contains the main P1 test collection.
- **environments/**: Holds global environment configurations for different environments (DEV, INT).


This organization allows you to run all API tests systematically while keeping each API’s tests separated.

---

## How to Use

### Load the Collection in Bruno
1. Open Bruno.  
2. In the **Collections** pane, click the **+** symbol next to **Collections** → **Open Collection**.  
3. Navigate to `Bruno_MPI_API_Tests/collections/P0_TestsCases` and select it.  

### Load the Environment in Bruno
1. Open the **Global Environments** tab.  
2. Load the environment you want to run tests in by :  
   - Using the `bruno-global-environments.json` file inside the `P0_TestsCases` folder.  

### Add Certificates in Bruno
1. Click on the **P0_TestsCases** collection → **Certificates** tab.  
2. Add the required client certificates for DEV and INT environments.  

### Run the Collection
1. After selecting the environment and adding certificates, run the collection.  
2. All API subfolders and test cases under **P0_TestsCases** will execute according to configuration.