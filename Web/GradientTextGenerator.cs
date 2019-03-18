namespace BillionSongs {
    using System;
    using System.Linq;
    using Gradient.Samples.GPT2;
    using JetBrains.Annotations;
    using numpy;
    using Python.Runtime;
    using SharPy.Runtime;
    using tensorflow;
    using tensorflow.train;

    public class GradientTextGenerator {
        const int BatchSize = 1;
        readonly Gpt2Encoder encoder;
        readonly Session session;
        readonly ndarray endOfText;
        readonly Tensor contextPlaceholder;
        readonly dynamic sampleOp;

        public GradientTextGenerator([NotNull] string modelName, [NotNull] string checkpoint, int sampleLength) {
            if (string.IsNullOrEmpty(modelName)) throw new ArgumentNullException(nameof(modelName));
            if (string.IsNullOrEmpty(checkpoint)) throw new ArgumentNullException(nameof(checkpoint));
            if (sampleLength < 0 || sampleLength > 1024*1024)
                throw new ArgumentOutOfRangeException(nameof(sampleLength));
            this.encoder = Gpt2Encoder.LoadEncoder(modelName);
            this.endOfText = np.array(new[] { this.encoder.EndOfText });
            var hParams = Gpt2Model.LoadHParams(modelName);

            this.session = new Session();
            this.session.__enter__();
            this.contextPlaceholder = tf.placeholder(tf.int32, new TensorShape(BatchSize, null));
            this.sampleOp = Gpt2Sampler.SampleSequence(
                hParams,
                length: sampleLength,
                context: this.contextPlaceholder,
                batchSize: BatchSize,
                temperature: 1.0f,
                topK: 40);

            var trainVars = tf.trainable_variables().Where((dynamic var) => var.name.Contains("model"));
            var saver = new Saver(
                var_list: trainVars,
                max_to_keep: 5,
                keep_checkpoint_every_n_hours: 1);

            this.session.run(tf.global_variables_initializer());

            this.session.graph.finalize();

            saver.restore(this.session, checkpoint);
        }

        public string GenerateSample(uint seed) {
            tf.set_random_seed(unchecked((int)seed));
            dynamic @out;
            using(Py.GIL())
                @out = this.session.run(this.sampleOp, feed_dict: new PythonDict<object, object> {
                    [this.contextPlaceholder] = new[] { this.endOfText },
                });
            return this.encoder.Decode(@out[0]);
        }
    }
}
