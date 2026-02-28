module.exports = {
  commands: [],
  platforms: {
    ios: {
      projectDirectory: './ios',
    },
  },
  project: {
    ios: {
      sourceDir: './ios',
      xcodeProject: {
        name: 'Spendly.xcworkspace',
        isWorkspace: true,
      },
    },
  },
};

