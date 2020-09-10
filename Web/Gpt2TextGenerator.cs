namespace BillionSongs {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LostTech.Gradient.Samples.GPT2;
    using JetBrains.Annotations;
    using numpy;
    using Python.Runtime;
    using tensorflow;
    using tensorflow.train;

    public class Gpt2TextGenerator {
        const int BatchSize = 1;
        const int MaxSampleLength = 1024;
        readonly Gpt2Encoder encoder;
        readonly ndarray endOfText;
        readonly string modelName;
        readonly int sampleLength;
        readonly string checkpoint;

        public string EndOfText => Gpt2Encoder.EndOfTextPseudoToken;

        public Gpt2TextGenerator([NotNull] string modelName, [NotNull] string checkpoint, int sampleLength) {
            if (string.IsNullOrEmpty(modelName)) throw new ArgumentNullException(nameof(modelName));
            if (string.IsNullOrEmpty(checkpoint)) throw new ArgumentNullException(nameof(checkpoint));
            if (sampleLength < 0 || sampleLength > 1024*1024)
                throw new ArgumentOutOfRangeException(nameof(sampleLength));
            this.checkpoint = checkpoint;
            this.sampleLength = sampleLength;
            this.modelName = modelName;
            this.encoder = Gpt2Encoder.LoadEncoder(modelName);
            this.endOfText = np.array(new[] { this.encoder.EncodedEndOfText });
        }

        public string GenerateSample(uint seed) {
            string sample = null;
            using (Py.GIL()) {
                tf.set_random_seed(unchecked((int)seed));
                new Session().UseSelf(session => {
                    var contextPlaceholder =
                        tf.placeholder(tf.int32, new TensorShape(BatchSize, null));
                    var hParams = Gpt2Model.LoadHParams(this.modelName);

                    var sampleOp = Gpt2Sampler.SampleSequence(
                        hParams,
                        length: Math.Min(this.sampleLength, MaxSampleLength),
                        context: contextPlaceholder,
                        batchSize: BatchSize,
                        temperature: 1.0f,
                        topK: 40);

                    IEnumerable<Variable> trainableVariables = tf.trainable_variables();
                    var trainVars = trainableVariables.Where(var => var.name.Contains("model"));
                    var saver = new Saver(
                        var_list: trainVars,
                        max_to_keep: 5,
                        keep_checkpoint_every_n_hours: 1);

                    session.run(tf.global_variables_initializer());

                    saver.restore(session, this.checkpoint);

                    var result = new StringBuilder(this.sampleLength);
                    while (result.Length < this.sampleLength) {
                        var @out = session.run(sampleOp, feed_dict: new Dictionary<object, object> {
                            [contextPlaceholder] = new[] { this.endOfText },
                        });
                        string chunk = this.encoder.Decode((ndarray<int>)@out[0]);
                        result.Append(chunk);
                    }

                    sample = result.ToString(0, this.sampleLength);
                });
                tf.reset_default_graph();
            }

            return sample;
        }
    }
}
