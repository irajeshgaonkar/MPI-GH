const fs = require('fs');
const path = require('path');

const repoRoot = path.resolve(__dirname, '..');
const distRoot = path.join(repoRoot, 'dist', 'hca-poc');
const filesToCopy = [
  'package.json',
  'package-lock.json',
  '.npmrc',
  'Procfile',
  'server.js',
];

if (!fs.existsSync(distRoot)) {
  console.error(`Build output not found: ${distRoot}`);
  process.exit(1);
}

for (const fileName of filesToCopy) {
  const sourcePath = path.join(repoRoot, fileName);
  const destinationPath = path.join(distRoot, fileName);

  if (!fs.existsSync(sourcePath)) {
    console.error(`Required runtime file not found: ${sourcePath}`);
    process.exit(1);
  }

  fs.copyFileSync(sourcePath, destinationPath);
}

console.log(`Copied runtime files to ${distRoot}`);