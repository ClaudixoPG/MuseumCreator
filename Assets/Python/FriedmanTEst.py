#implement friedamn test for the data
import numpy as np
import scipy.stats as stats
import pandas as pd
import os

def friedman_test(data):
    """
    This function implements the Friedman test for repeated measures.
    The function takes a list of numpy arrays as input where each array
    is a set of measurements of the same units under different conditions.
    The function returns the test statistic and the associated p-value.
    """
    k = len(data)
    n = len(data[0])
    
    # Rank data
    ranked_data = np.zeros((k, n))
    for i in range(n):
        ranked_data[:, i] = stats.rankdata([data[j][i] for j in range(k)])
    
    # Calculate the test statistic
    friedman_stat = 12 / (n * k * (k + 1)) * np.sum(ranked_data.sum(axis=0)**2) - 3 * n * (k + 1)

    # Calculate the p-value
    p_value = 1 - stats.chi2.cdf(friedman_stat, k - 1)

    return friedman_stat, p_value

def nemenyi(data):
    """
    This function implements the Nemenyi post-hoc test for the Friedman test.
    The function takes a list of numpy arrays as input where each array
    is a set of measurements of the same units under different conditions.
    The function returns a pandas DataFrame with the critical difference
    values for the Nemenyi test at the 0.05 significance level.
    """
    k = len(data)
    n = len(data[0])
    
    # Calculate the critical difference values
    q = stats.norm.ppf(1 - 0.05 / (k * (k - 1))**0.5)
    cd = q * (k * (k + 1) / (6 * n))**0.5

    # Create a pandas DataFrame
    columns = ['Group {}'.format(i + 1) for i in range(k)]
    df = pd.DataFrame(index=columns, columns=columns)
    
    for i in range(k):
        for j in range(k):
            if i == j:
                df.iloc[i, j] = '-'
            else:
                diff = np.abs(np.mean(data[i]) - np.mean(data[j]))
                if diff > cd:
                    df.iloc[i, j] = 'Yes'
                else:
                    df.iloc[i, j] = 'No'
    
    return df

def load_csv_as_strings(file_path):
    """
    This function reads a CSV file and treats each cell as a string, preserving internal commas.
    """
    try:
        # Read the CSV file without any delimiter enforcement
        data = pd.read_csv(file_path, sep=None, engine='python', dtype=str)
        return data
    except Exception as e:
        print(f"Error loading data from {file_path}: {e}")
        return None

# Example usage
file_path = 'C:/Users/claud/Documents/GitHub/Prototypes/MuseumOptimization/MuseumCreator/Assets/SpaceOptimization/Resources/Maps/TSP_DATA_11x11.csv'
data = load_csv_as_strings(file_path)

if data is not None:
    print("Data loaded successfully.")
    print(data.head())  # Display the first few rows of the dataframe
else:
    print("Failed to load data.")

#print values in columns  ACOTourLength	ACOTour SATourLength	SATour	PSOTourLength	PSOTour	

df = data[['ACOTourLength', 'SATourLength', 'PSOTourLength']]

#convert columns to  list of numpy arrays for the friedman test
data = [df[col].values.astype(float) for col in df.columns]

#print the data values
for i in range(len(data)):
    print(f'Group {i + 1}: {data[i]}')

# Example usage

#perform the friedman test
stat, p = friedman_test(data)
print('Friedman test statistic:', stat)
print('P-value:', p)

#print if are different or not, if p-value is less than 0.05 then they are different
if p < 0.05:
    print('There is a significant difference between the groups.')
else:
    print('There is no significant difference between the groups.')

#if are different then we can use post-hoc test to see which group is different from the others
#we can use the Nemenyi test for this purpose

# Calculate the critical difference values for the example data
cd = nemenyi(data)
print('\nCritical difference values:')
print(cd)


